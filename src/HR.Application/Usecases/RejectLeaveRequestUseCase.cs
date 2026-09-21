using HR.Application.DTOs;
using HR.Application.Exceptions;
using HR.Application.Interfaces;
using HR.Application.Mapping;
using HR.Domain.Entities;

namespace HR.Application.UseCases;

public class RejectLeaveRequestUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public RejectLeaveRequestUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<LeaveRequestDto> ExecuteAsync(Guid leaveRequestId, Guid managerId, string? comment)
    {
        var request = await _unitOfWork.LeaveRequests.GetByIdAsync(leaveRequestId)
            ?? throw new NotFoundException(nameof(LeaveRequest), leaveRequestId);

        var requester = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId)
            ?? throw new NotFoundException(nameof(Employee), request.EmployeeId);

        var manager = await _unitOfWork.Employees.GetByIdAsync(managerId)
            ?? throw new NotFoundException(nameof(Employee), managerId);

        if (!manager.IsDirectManagerOf(requester))
            throw new ForbiddenException("You can only reject requests from your direct reports.");

        // FR10: no balance deduction on rejection.
        request.Reject(managerId, comment);

        await _unitOfWork.SaveChangesAsync();

        return LeaveRequestMapper.ToDto(request);
    }
}