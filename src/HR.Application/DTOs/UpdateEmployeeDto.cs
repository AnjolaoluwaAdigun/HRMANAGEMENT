namespace HR.Application.DTOs;

public record UpdateEmployeeDto(string Name, string Email, string Department, Guid? ManagerId);