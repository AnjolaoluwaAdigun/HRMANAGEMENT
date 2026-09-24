using HR.Application.DTOs;
using HR.Application.Interfaces;
using HR.Domain.Enums;

namespace HR.Application.UseCases;

public class GetCompanyLeaveStatsUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCompanyLeaveStatsUseCase(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    // FR15, US-FE11 — company-wide visibility for HR Admin: who's on leave today,
    // and approved-day usage broken down by department for the current year.
    public async Task<CompanyLeaveStatsDto> ExecuteAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentYear = today.Year;

        var employees = await _unitOfWork.Employees.GetAllAsync();
        var activeEmployees = employees.Where(e => e.IsActive).ToList();

        var approvedRequests = await _unitOfWork.LeaveRequests.GetByStatusAsync(RequestStatus.Approved);
        var leaveTypes = await _unitOfWork.LeaveTypes.GetAllAsync();
        var leaveTypeNames = leaveTypes.ToDictionary(t => t.LeaveTypeId, t => t.Name);
        var employeesById = activeEmployees.ToDictionary(e => e.EmployeeId, e => e);

        // Who's on leave right now: approved requests whose date range includes today.
        var onLeaveToday = approvedRequests
            .Where(r => r.StartDate <= today && r.EndDate >= today && employeesById.ContainsKey(r.EmployeeId))
            .Select(r =>
            {
                var employee = employeesById[r.EmployeeId];
                return new EmployeeOnLeaveDto(
                    employee.EmployeeId,
                    employee.Name,
                    employee.Department,
                    leaveTypeNames.GetValueOrDefault(r.LeaveTypeId, "Unknown"),
                    r.StartDate,
                    r.EndDate);
            })
            .ToList();

        // Department-level usage: total approved days taken this year, grouped by department.
        var departmentUsage = approvedRequests
            .Where(r => r.StartDate.Year == currentYear && employeesById.ContainsKey(r.EmployeeId))
            .GroupBy(r => employeesById[r.EmployeeId].Department)
            .Select(g => new DepartmentUsageDto(
                Department: g.Key,
                EmployeeCount: g.Select(r => r.EmployeeId).Distinct().Count(),
                TotalApprovedDaysThisYear: g.Sum(r => r.RequestedDays)))
            .OrderByDescending(d => d.TotalApprovedDaysThisYear)
            .ToList();

        return new CompanyLeaveStatsDto(
            TotalEmployees: activeEmployees.Count,
            EmployeesOnLeaveToday: onLeaveToday.Count,
            CurrentlyOnLeave: onLeaveToday,
            DepartmentUsage: departmentUsage);
    }
}