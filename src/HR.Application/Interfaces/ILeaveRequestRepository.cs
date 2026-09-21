using HR.Domain.Entities;
using HR.Domain.Enums;

namespace HR.Application.Interfaces;

public interface ILeaveRequestRepository
{
    Task<LeaveRequest?> GetByIdAsync(Guid leaveRequestId);

    // FR6: used to check for overlaps against existing Pending/Approved requests
    // for the same employee before allowing a new submission.
    Task<List<LeaveRequest>> GetActiveByEmployeeAsync(Guid employeeId);

    Task<List<LeaveRequest>> GetByEmployeeAsync(Guid employeeId);

    // US5 / FR14: approved requests only, for a manager's direct reports, in a date range.
    Task<List<LeaveRequest>> GetApprovedForEmployeesAsync(
        List<Guid> employeeIds, DateOnly rangeStart, DateOnly rangeEnd);

    // FR15: company-wide reporting.
    Task<List<LeaveRequest>> GetByStatusAsync(RequestStatus status);

    Task AddAsync(LeaveRequest leaveRequest);
}