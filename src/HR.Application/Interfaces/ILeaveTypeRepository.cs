using HR.Domain.Entities;

namespace HR.Application.Interfaces;

public interface ILeaveTypeRepository
{
    Task<LeaveType?> GetByIdAsync(Guid leaveTypeId);
    Task<List<LeaveType>> GetAllAsync();
    Task AddAsync(LeaveType leaveType);
}