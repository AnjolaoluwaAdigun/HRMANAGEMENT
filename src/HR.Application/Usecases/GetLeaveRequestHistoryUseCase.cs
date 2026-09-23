using HR.Application.DTOs;
using HR.Application.Interfaces;
using HR.Application.Mapping;

namespace HR.Application.UseCases;

public class GetLeaveRequestHistoryUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public GetLeaveRequestHistoryUseCase(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    // FR13 — an employee's full leave request history, all statuses, most recent first.
    public async Task<List<LeaveRequestDto>> ExecuteAsync(Guid employeeId)
    {
        var requests = await _unitOfWork.LeaveRequests.GetByEmployeeAsync(employeeId);
        return requests.Select(LeaveRequestMapper.ToDto).ToList();
    }
}
