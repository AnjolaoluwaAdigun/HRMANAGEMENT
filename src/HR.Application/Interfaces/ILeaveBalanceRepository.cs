using HR.Domain.Entities;

namespace HR.Application.Interfaces;

public interface ILeaveBalanceRepository
{
    Task<LeaveBalance?> GetAsync(Guid employeeId, Guid leaveTypeId, int year);
    Task<List<LeaveBalance>> GetForEmployeeAsync(Guid employeeId, int year);
    Task AddAsync(LeaveBalance balance);
}