using HR.Domain.Entities;

namespace HR.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid employeeId);
    Task<Employee?> GetByEmailAsync(string email);
    Task<List<Employee>> GetDirectReportsAsync(Guid managerId);
    Task<List<Employee>> GetAllAsync();
    Task AddAsync(Employee employee);
}