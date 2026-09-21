namespace HR.Application.DTOs;

public record LeaveBalanceDto(
    Guid LeaveTypeId,
    string LeaveTypeName,
    int Year,
    decimal RemainingDays);