using HR.Application.UseCases;
using HR.Domain.Entities;
using HR.Domain.Enums;
using Moq;
using Xunit;

namespace HR.Application.Tests;

public class GetTeamLeaveCalendarUseCaseTests
{
    [Fact]
    public async Task Execute_ReturnsOnlyApprovedLeaveForDirectReports()
    {
        var builder = new TestUnitOfWorkBuilder();
        var managerId = Guid.NewGuid();
        var report1 = new Employee("A", "a@co.com", "Eng", new DateOnly(2024, 1, 1), Role.Employee, "hash", managerId);
        var report2 = new Employee("B", "b@co.com", "Eng", new DateOnly(2024, 1, 1), Role.Employee, "hash", managerId);

        var approvedRequest = new LeaveRequest(
            report1.EmployeeId, Guid.NewGuid(),
            new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 9), 5, "Trip");
        approvedRequest.Approve(managerId);

        builder.Employees.Setup(r => r.GetDirectReportsAsync(managerId))
            .ReturnsAsync(new List<Employee> { report1, report2 });
        builder.LeaveRequests.Setup(r => r.GetApprovedForEmployeesAsync(
                It.IsAny<List<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(new List<LeaveRequest> { approvedRequest });

        var useCase = new GetTeamLeaveCalendarUseCase(builder.Build());
        var result = await useCase.ExecuteAsync(managerId, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));

        Assert.Single(result);
        Assert.Equal(RequestStatus.Approved, result[0].Status);
    }
}