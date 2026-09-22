using HR.Application.Interfaces;
using HR.Domain.Entities;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Repositories;

public class LeaveTypeRepository : ILeaveTypeRepository
{
    private readonly ApplicationDbContext _context;

    public LeaveTypeRepository(ApplicationDbContext context) => _context = context;

    public Task<LeaveType?> GetByIdAsync(Guid leaveTypeId) =>
        _context.LeaveTypes.FirstOrDefaultAsync(t => t.LeaveTypeId == leaveTypeId);

    public async Task<List<LeaveType>> GetAllAsync() =>
        await _context.LeaveTypes.ToListAsync();

    public async Task AddAsync(LeaveType leaveType) =>
        await _context.LeaveTypes.AddAsync(leaveType);
}