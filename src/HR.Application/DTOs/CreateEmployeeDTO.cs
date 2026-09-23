using HR.Domain.Enums;

namespace HR.Application.DTOs;

public record CreateEmployeeDto(
    string Name,
    string Email,
    string Department,
    DateOnly DateJoined,
    Role Role,
    string Password,
    Guid? ManagerId);