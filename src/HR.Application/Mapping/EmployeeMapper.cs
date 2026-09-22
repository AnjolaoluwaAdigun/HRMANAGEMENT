using HR.Application.DTOs;
using HR.Domain.Entities;

namespace HR.Application.Mapping;

public static class EmployeeMapper
{
    public static EmployeeDto ToDto(Employee employee) => new(
        employee.EmployeeId,
        employee.Name,
        employee.Email,
        employee.Department,
        employee.ManagerId,
        employee.DateJoined,
        employee.Role,
        employee.Status);
}