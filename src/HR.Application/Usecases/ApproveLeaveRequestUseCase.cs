using HR.Application.DTOs;
using HR.Application.Exceptions;
using HR.Application.Interfaces;
using HR.Application.Mapping;
using HR.Domain.Entities;

namespace HR.Application.UseCases;

public class ApproveLeaveRequestUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public ApproveLeaveRequestUseCase(IUnitOfWork unitOfWork)
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

        // US2: a manager may only act on their own direct reports' requests.
        if (!manager.IsDirectManagerOf(requester))
            throw new ForbiddenException("You can only approve requests from your direct reports.");

        var currentYear = request.StartDate.Year;
        var balance = await _unitOfWork.LeaveBalances.GetAsync(request.EmployeeId, request.LeaveTypeId, currentYear)
            ?? throw new NotFoundException("LeaveBalance for this employee/type/year", request.EmployeeId);

        // Both mutations happen against the same tracked context; one SaveChangesAsync
        // commits them together, so a failed deduction also rolls back the approval.
        balance.Deduct(request.RequestedDays);
        request.Approve(managerId, comment);

        await _unitOfWork.SaveChangesAsync();

        return LeaveRequestMapper.ToDto(request);
    }
}