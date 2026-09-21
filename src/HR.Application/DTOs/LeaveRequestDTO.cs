using HR.Domain.Enums;

namespace HR.Application.DTOs;

public record LeaveRequestDto(
    Guid LeaveRequestId,
    Guid EmployeeId,
    Guid LeaveTypeId,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal RequestedDays,
    string Reason,
    RequestStatus Status,
    Guid? ApprovedBy,
    string? ApprovalComment,
    DateTime CreatedAt);