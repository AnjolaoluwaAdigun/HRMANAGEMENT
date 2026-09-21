namespace HR.Application.Interfaces;

// Ties all repositories to a single underlying DbContext instance, so a use case
// can modify several entities (e.g. LeaveRequest + LeaveBalance) and persist them
// in one SaveChangesAsync call — one atomic transaction, satisfying NFR-Data Integrity (FR10).
public interface IUnitOfWork
{
    IEmployeeRepository Employees { get; }
    ILeaveTypeRepository LeaveTypes { get; }
    ILeaveBalanceRepository LeaveBalances { get; }
    ILeaveRequestRepository LeaveRequests { get; }

    Task<int> SaveChangesAsync();
}