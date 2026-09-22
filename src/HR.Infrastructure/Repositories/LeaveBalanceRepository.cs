using HR.Application.Interfaces;
using HR.Domain.Entities;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Repositories;

public class LeaveBalanceRepository : ILeaveBalanceRepository
{
    private readonly ApplicationDbContext _context;

    public LeaveBalanceRepository(ApplicationDbContext context) => _context = context;

    public Task<LeaveBalance?> GetAsync(Guid employeeId, Guid leaveTypeId, int year) =>
        _context.LeaveBalances.FirstOrDefaultAsync(b =>
            b.EmployeeId == employeeId && b.LeaveTypeId == leaveTypeId && b.Year == year);

    public async Task<List<LeaveBalance>> GetForEmployeeAsync(Guid employeeId, int year) =>
        await _context.LeaveBalances
            .Where(b => b.EmployeeId == employeeId && b.Year == year)
            .ToListAsync();

    public async Task AddAsync(LeaveBalance balance) =>
        await _context.LeaveBalances.AddAsync(balance);
}