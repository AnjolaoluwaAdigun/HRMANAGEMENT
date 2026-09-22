using HR.Application.DTOs;
using HR.Application.Exceptions;
using HR.Application.UseCases;
using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Domain.Exceptions;
using Moq;
using Xunit;

namespace HR.Application.Tests;

public class SubmitLeaveRequestUseCaseTests
{
    private static Employee CreateActiveEmployee() =>
        new("Jane Doe", "jane@company.com", "Engineering", new DateOnly(2024, 1, 1),
            Role.Employee, "hashed-password");

    private static LeaveType CreateLeaveType() => new("Annual", 20);

    [Fact]
    public async Task Execute_ValidRequest_CreatesRequestAndSucceeds()
    {
        var builder = new TestUnitOfWorkBuilder();
        var employee = CreateActiveEmployee();
        var leaveType = CreateLeaveType();
        var balance = new LeaveBalance(employee.EmployeeId, leaveType.LeaveTypeId, 2026, initialDays: 20);

        builder.Employees.Setup(r => r.GetByIdAsync(employee.EmployeeId)).ReturnsAsync(employee);
        builder.LeaveTypes.Setup(r => r.GetByIdAsync(leaveType.LeaveTypeId)).ReturnsAsync(leaveType);
        builder.LeaveRequests.Setup(r => r.GetActiveByEmployeeAsync(employee.EmployeeId))
            .ReturnsAsync(new List<LeaveRequest>());
        builder.LeaveBalances.Setup(r => r.GetAsync(employee.EmployeeId, leaveType.LeaveTypeId, 2026))
            .ReturnsAsync(balance);

        var useCase = new SubmitLeaveRequestUseCase(builder.Build());
        var dto = new SubmitLeaveRequestDto(
            employee.EmployeeId, leaveType.LeaveTypeId,
            new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 9), "Family trip");

        var result = await useCase.ExecuteAsync(dto);

        Assert.Equal(RequestStatus.Pending, result.Status);
        builder.LeaveRequests.Verify(r => r.AddAsync(It.IsAny<LeaveRequest>()), Times.Once);
    }

    [Fact]
    public async Task Execute_InsufficientBalance_ThrowsInsufficientBalanceException()
    {
        var builder = new TestUnitOfWorkBuilder();
        var employee = CreateActiveEmployee();
        var leaveType = CreateLeaveType();
        // Only 2 days available, but Mon–Fri (5 weekdays) will be requested below.
        var balance = new LeaveBalance(employee.EmployeeId, leaveType.LeaveTypeId, 2026, initialDays: 2);

        builder.Employees.Setup(r => r.GetByIdAsync(employee.EmployeeId)).ReturnsAsync(employee);
        builder.LeaveTypes.Setup(r => r.GetByIdAsync(leaveType.LeaveTypeId)).ReturnsAsync(leaveType);
        builder.LeaveRequests.Setup(r => r.GetActiveByEmployeeAsync(employee.EmployeeId))
            .ReturnsAsync(new List<LeaveRequest>());
        builder.LeaveBalances.Setup(r => r.GetAsync(employee.EmployeeId, leaveType.LeaveTypeId, 2026))
            .ReturnsAsync(balance);

        var useCase = new SubmitLeaveRequestUseCase(builder.Build());
        // Mon 5th to Fri 9th Oct 2026 = 5 weekdays.
        var dto = new SubmitLeaveRequestDto(
            employee.EmployeeId, leaveType.LeaveTypeId,
            new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 9), "Family trip");

        await Assert.ThrowsAsync<InsufficientBalanceException>(() => useCase.ExecuteAsync(dto));

        // Nothing should be persisted when validation fails.
        builder.LeaveRequests.Verify(r => r.AddAsync(It.IsAny<LeaveRequest>()), Times.Never);
    }

    [Fact]
    public async Task Execute_OverlappingRequestExists_ThrowsOverlappingLeaveRequestException()
    {
        var builder = new TestUnitOfWorkBuilder();
        var employee = CreateActiveEmployee();
        var leaveType = CreateLeaveType();
        var existingRequest = new LeaveRequest(
            employee.EmployeeId, leaveType.LeaveTypeId,
            new DateOnly(2026, 10, 6), new DateOnly(2026, 10, 8), 3, "Existing trip");

        builder.Employees.Setup(r => r.GetByIdAsync(employee.EmployeeId)).ReturnsAsync(employee);
        builder.LeaveTypes.Setup(r => r.GetByIdAsync(leaveType.LeaveTypeId)).ReturnsAsync(leaveType);
        builder.LeaveRequests.Setup(r => r.GetActiveByEmployeeAsync(employee.EmployeeId))
            .ReturnsAsync(new List<LeaveRequest> { existingRequest });

        var useCase = new SubmitLeaveRequestUseCase(builder.Build());
        // Overlaps the existing 6th–8th request.
        var dto = new SubmitLeaveRequestDto(
            employee.EmployeeId, leaveType.LeaveTypeId,
            new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 9), "Overlapping trip");

        await Assert.ThrowsAsync<OverlappingLeaveRequestException>(() => useCase.ExecuteAsync(dto));
    }

    [Fact]
    public async Task Execute_InactiveEmployee_ThrowsForbiddenException()
    {
        var builder = new TestUnitOfWorkBuilder();
        var employee = CreateActiveEmployee();
        employee.Deactivate();
        var leaveType = CreateLeaveType();

        builder.Employees.Setup(r => r.GetByIdAsync(employee.EmployeeId)).ReturnsAsync(employee);

        var useCase = new SubmitLeaveRequestUseCase(builder.Build());
        var dto = new SubmitLeaveRequestDto(
            employee.EmployeeId, leaveType.LeaveTypeId,
            new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 9), "Trip");

        await Assert.ThrowsAsync<ForbiddenException>(() => useCase.ExecuteAsync(dto));
    }

    [Fact]
    public async Task Execute_EmployeeNotFound_ThrowsNotFoundException()
    {
        var builder = new TestUnitOfWorkBuilder();
        var missingEmployeeId = Guid.NewGuid();

        builder.Employees.Setup(r => r.GetByIdAsync(missingEmployeeId))
            .ReturnsAsync((Employee?)null);

        var useCase = new SubmitLeaveRequestUseCase(builder.Build());
        var dto = new SubmitLeaveRequestDto(
            missingEmployeeId, Guid.NewGuid(),
            new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 9), "Trip");

        await Assert.ThrowsAsync<NotFoundException>(() => useCase.ExecuteAsync(dto));
    }
}