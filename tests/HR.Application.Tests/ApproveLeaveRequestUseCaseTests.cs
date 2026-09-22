using HR.Application.Exceptions;
using HR.Application.UseCases;
using HR.Domain.Entities;
using HR.Domain.Enums;
using Moq;
using Xunit;

namespace HR.Application.Tests;

public class ApproveLeaveRequestUseCaseTests
{
    private static Employee CreateEmployee(string name = "Jane Doe", Guid? managerId = null) =>
        new(name, $"{name.Replace(" ", ".").ToLower()}@company.com", "Engineering",
            new DateOnly(2024, 1, 1), Role.Employee, "hashed-password", managerId);

    [Fact]
    public async Task Execute_ManagerApprovesDirectReport_ApprovesAndDeductsBalance()
    {
        var builder = new TestUnitOfWorkBuilder();
        var manager = CreateEmployee("Sam Manager");
        var employee = CreateEmployee("Jane Doe", manager.EmployeeId);
        var leaveTypeId = Guid.NewGuid();
        var request = new LeaveRequest(
            employee.EmployeeId, leaveTypeId,
            new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 9), requestedDays: 5, reason: "Trip");
        var balance = new LeaveBalance(employee.EmployeeId, leaveTypeId, 2026, initialDays: 20);

        builder.LeaveRequests.Setup(r => r.GetByIdAsync(request.LeaveRequestId)).ReturnsAsync(request);
        builder.Employees.Setup(r => r.GetByIdAsync(employee.EmployeeId)).ReturnsAsync(employee);
        builder.Employees.Setup(r => r.GetByIdAsync(manager.EmployeeId)).ReturnsAsync(manager);
        builder.LeaveBalances.Setup(r => r.GetAsync(employee.EmployeeId, leaveTypeId, 2026)).ReturnsAsync(balance);

        var useCase = new ApproveLeaveRequestUseCase(builder.Build());

        var result = await useCase.ExecuteAsync(request.LeaveRequestId, manager.EmployeeId, "Enjoy!");

        Assert.Equal(RequestStatus.Approved, result.Status);
        Assert.Equal(15, balance.RemainingDays); // 20 - 5, deducted atomically alongside approval
    }

    // This is the core US2 rule: a manager cannot approve someone who isn't
    // their direct report. Section 9.1 requires this covered by automated tests.
    [Fact]
    public async Task Execute_ManagerApprovesNonDirectReport_ThrowsForbiddenException()
    {
        var builder = new TestUnitOfWorkBuilder();
        var actualManager = CreateEmployee("Real Manager");
        var unrelatedManager = CreateEmployee("Unrelated Manager");
        var employee = CreateEmployee("Jane Doe", actualManager.EmployeeId);
        var leaveTypeId = Guid.NewGuid();
        var request = new LeaveRequest(
            employee.EmployeeId, leaveTypeId,
            new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 9), requestedDays: 5, reason: "Trip");

        builder.LeaveRequests.Setup(r => r.GetByIdAsync(request.LeaveRequestId)).ReturnsAsync(request);
        builder.Employees.Setup(r => r.GetByIdAsync(employee.EmployeeId)).ReturnsAsync(employee);
        builder.Employees.Setup(r => r.GetByIdAsync(unrelatedManager.EmployeeId)).ReturnsAsync(unrelatedManager);

        var useCase = new ApproveLeaveRequestUseCase(builder.Build());

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            useCase.ExecuteAsync(request.LeaveRequestId, unrelatedManager.EmployeeId, null));

        // The request must remain untouched — no partial approval.
        Assert.Equal(RequestStatus.Pending, request.Status);
    }

    [Fact]
    public async Task Execute_InsufficientBalanceAtApprovalTime_ThrowsAndDoesNotApprove()
    {
        var builder = new TestUnitOfWorkBuilder();
        var manager = CreateEmployee("Sam Manager");
        var employee = CreateEmployee("Jane Doe", manager.EmployeeId);
        var leaveTypeId = Guid.NewGuid();
        var request = new LeaveRequest(
            employee.EmployeeId, leaveTypeId,
            new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 9), requestedDays: 5, reason: "Trip");
        // Balance dropped below the requested amount since submission (e.g. another
        // request was approved in between) — approval must fail, not silently go negative.
        var balance = new LeaveBalance(employee.EmployeeId, leaveTypeId, 2026, initialDays: 2);

        builder.LeaveRequests.Setup(r => r.GetByIdAsync(request.LeaveRequestId)).ReturnsAsync(request);
        builder.Employees.Setup(r => r.GetByIdAsync(employee.EmployeeId)).ReturnsAsync(employee);
        builder.Employees.Setup(r => r.GetByIdAsync(manager.EmployeeId)).ReturnsAsync(manager);
        builder.LeaveBalances.Setup(r => r.GetAsync(employee.EmployeeId, leaveTypeId, 2026)).ReturnsAsync(balance);

        var useCase = new ApproveLeaveRequestUseCase(builder.Build());

        await Assert.ThrowsAsync<HR.Domain.Exceptions.InsufficientBalanceException>(() =>
            useCase.ExecuteAsync(request.LeaveRequestId, manager.EmployeeId, null));

        // Neither the balance nor the request status should change on failure.
        Assert.Equal(2, balance.RemainingDays);
        Assert.Equal(RequestStatus.Pending, request.Status);
    }
}