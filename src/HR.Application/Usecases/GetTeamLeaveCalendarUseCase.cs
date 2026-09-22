using HR.Application.DTOs;
using HR.Application.Interfaces;
using HR.Application.Mapping;

namespace HR.Application.UseCases;

public class GetTeamLeaveCalendarUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTeamLeaveCalendarUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<LeaveRequestDto>> ExecuteAsync(Guid managerId, DateOnly rangeStart, DateOnly rangeEnd)
    {
        var directReports = await _unitOfWork.Employees.GetDirectReportsAsync(managerId);
        var reportIds = directReports.Select(e => e.EmployeeId).ToList();

        // US5: only approved leave, never pending or rejected, for direct reports only.
        var approvedRequests = await _unitOfWork.LeaveRequests
            .GetApprovedForEmployeesAsync(reportIds, rangeStart, rangeEnd);

        return approvedRequests.Select(LeaveRequestMapper.ToDto).ToList();
    }
}