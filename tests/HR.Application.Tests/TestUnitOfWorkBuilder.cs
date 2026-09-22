using HR.Application.Interfaces;
using Moq;

namespace HR.Application.Tests;

// Bundles the four repository mocks + IUnitOfWork mock together so individual
// tests can configure just the repos they care about, without repeating this
// wiring in every test file.
public class TestUnitOfWorkBuilder
{
    public Mock<IEmployeeRepository> Employees { get; } = new();
    public Mock<ILeaveTypeRepository> LeaveTypes { get; } = new();
    public Mock<ILeaveBalanceRepository> LeaveBalances { get; } = new();
    public Mock<ILeaveRequestRepository> LeaveRequests { get; } = new();

    public IUnitOfWork Build()
    {
        var uow = new Mock<IUnitOfWork>();
        uow.Setup(u => u.Employees).Returns(Employees.Object);
        uow.Setup(u => u.LeaveTypes).Returns(LeaveTypes.Object);
        uow.Setup(u => u.LeaveBalances).Returns(LeaveBalances.Object);
        uow.Setup(u => u.LeaveRequests).Returns(LeaveRequests.Object);
        uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        return uow.Object;
    }
}