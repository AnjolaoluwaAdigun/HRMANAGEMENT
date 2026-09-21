using HR.Application.DTOs;
using HR.Domain.Entities;

namespace HR.Application.Mapping;

public static class LeaveRequestMapper
{
    public static LeaveRequestDto ToDto(LeaveRequest request) => new(
        request.LeaveRequestId,
        request.EmployeeId,
        request.LeaveTypeId,
        request.StartDate,
        request.EndDate,
        request.RequestedDays,
        request.Reason,
        request.Status,
        request.ApprovedBy,
        request.ApprovalComment,
        request.CreatedAt);
}