using HR.Application.DTOs;
using HR.Application.Interfaces;
using HR.Domain.Entities;

namespace HR.Application.UseCases;

public class CreateLeaveTypeUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeaveTypeUseCase(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<LeaveTypeDto> ExecuteAsync(CreateLeaveTypeDto dto)
    {
        var leaveType = new LeaveType(dto.Name, dto.DefaultDaysPerYear);
        await _unitOfWork.LeaveTypes.AddAsync(leaveType);
        await _unitOfWork.SaveChangesAsync();

        return new LeaveTypeDto(leaveType.LeaveTypeId, leaveType.Name, leaveType.DefaultDaysPerYear);
    }
}