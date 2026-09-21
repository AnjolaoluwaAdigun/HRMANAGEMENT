namespace HR.Application.DTOs;

public record SubmitLeaveRequestDto(
    Guid EmployeeId,
    Guid LeaveTypeId,
    DateOnly StartDate,
    DateOnly EndDate,
    string Reason);