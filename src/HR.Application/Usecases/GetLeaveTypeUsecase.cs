using HR.Application.DTOs;
using HR.Application.Interfaces;

namespace HR.Application.UseCases;

public class GetLeaveTypesUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public GetLeaveTypesUseCase(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<List<LeaveTypeDto>> ExecuteAsync()
    {
        var leaveTypes = await _unitOfWork.LeaveTypes.GetAllAsync();
        return leaveTypes.Select(t => new LeaveTypeDto(t.LeaveTypeId, t.Name, t.DefaultDaysPerYear)).ToList();
    }
}