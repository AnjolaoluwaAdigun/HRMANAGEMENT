using HR.Application.Interfaces;
using HR.Domain.Entities;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context) => _context = context;

    public Task<Employee?> GetByIdAsync(Guid employeeId) =>
        _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

    public Task<Employee?> GetByEmailAsync(string email) =>
        _context.Employees.FirstOrDefaultAsync(e => e.Email == email);

    public async Task<List<Employee>> GetDirectReportsAsync(Guid managerId) =>
        await _context.Employees.Where(e => e.ManagerId == managerId).ToListAsync();

    public async Task<List<Employee>> GetAllAsync() =>
        await _context.Employees.ToListAsync();

    public async Task AddAsync(Employee employee) =>
        await _context.Employees.AddAsync(employee);
}