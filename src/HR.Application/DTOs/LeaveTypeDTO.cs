namespace HR.Application.DTOs;

public record LeaveTypeDto(Guid LeaveTypeId, string Name, int DefaultDaysPerYear);