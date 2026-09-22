using HR.Application.Interfaces;
using HR.Infrastructure.Repositories;

namespace HR.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IEmployeeRepository Employees { get; }
    public ILeaveTypeRepository LeaveTypes { get; }
    public ILeaveBalanceRepository LeaveBalances { get; }
    public ILeaveRequestRepository LeaveRequests { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Employees = new EmployeeRepository(context);
        LeaveTypes = new LeaveTypeRepository(context);
        LeaveBalances = new LeaveBalanceRepository(context);
        LeaveRequests = new LeaveRequestRepository(context);
    }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}