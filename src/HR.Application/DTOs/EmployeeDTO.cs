using HR.Domain.Enums;

namespace HR.Application.DTOs;

public record EmployeeDto(
    Guid EmployeeId,
    string Name,
    string Email,
    string Department,
    Guid? ManagerId,
    DateOnly DateJoined,
    Role Role,
    EmployeeStatus Status);