using HR.Application.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Repositories;

public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly ApplicationDbContext _context;

    public LeaveRequestRepository(ApplicationDbContext context) => _context = context;

    public Task<LeaveRequest?> GetByIdAsync(Guid leaveRequestId) =>
        _context.LeaveRequests.FirstOrDefaultAsync(r => r.LeaveRequestId == leaveRequestId);

    public async Task<List<LeaveRequest>> GetActiveByEmployeeAsync(Guid employeeId) =>
        await _context.LeaveRequests
            .Where(r => r.EmployeeId == employeeId &&
                        (r.Status == RequestStatus.Pending || r.Status == RequestStatus.Approved))
            .ToListAsync();

    public async Task<List<LeaveRequest>> GetByEmployeeAsync(Guid employeeId) =>
        await _context.LeaveRequests
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<List<LeaveRequest>> GetApprovedForEmployeesAsync(
        List<Guid> employeeIds, DateOnly rangeStart, DateOnly rangeEnd) =>
        await _context.LeaveRequests
            .Where(r => employeeIds.Contains(r.EmployeeId) &&
                        r.Status == RequestStatus.Approved &&
                        r.StartDate <= rangeEnd && r.EndDate >= rangeStart)
            .ToListAsync();

    public async Task<List<LeaveRequest>> GetByStatusAsync(RequestStatus status) =>
        await _context.LeaveRequests.Where(r => r.Status == status).ToListAsync();

    public async Task AddAsync(LeaveRequest leaveRequest) =>
        await _context.LeaveRequests.AddAsync(leaveRequest);
}