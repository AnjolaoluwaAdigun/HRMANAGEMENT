using HR.Application.DTOs;
using HR.Application.Exceptions;
using HR.Application.Interfaces;
using HR.Application.Mapping;
using HR.Domain.Entities;
using HR.Domain.Exceptions;
using HR.Domain.Services;

namespace HR.Application.UseCases;

public class SubmitLeaveRequestUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public SubmitLeaveRequestUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<LeaveRequestDto> ExecuteAsync(SubmitLeaveRequestDto dto)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(dto.EmployeeId)
            ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId);

        if (!employee.IsActive)
            throw new ForbiddenException("Inactive employees cannot submit leave requests.");

        var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(dto.LeaveTypeId)
            ?? throw new NotFoundException(nameof(Domain.Entities.LeaveType), dto.LeaveTypeId);

        // FR5: calculate requested days excluding weekends.
        var requestedDays = WorkingDayCalculator.CalculateWorkingDays(dto.StartDate, dto.EndDate);

        // FR6: reject if this overlaps any of the employee's existing active requests.
        var activeRequests = await _unitOfWork.LeaveRequests.GetActiveByEmployeeAsync(dto.EmployeeId);
        if (activeRequests.Any(r => r.OverlapsWith(dto.StartDate, dto.EndDate)))
            throw new OverlappingLeaveRequestException();

        // FR5: validate against remaining balance before allowing submission.
        var currentYear = dto.StartDate.Year;
        var balance = await _unitOfWork.LeaveBalances.GetAsync(dto.EmployeeId, dto.LeaveTypeId, currentYear)
            ?? throw new NotFoundException("LeaveBalance for this employee/type/year", dto.EmployeeId);

        if (!balance.HasSufficientBalance(requestedDays))
            throw new InsufficientBalanceException(balance.RemainingDays, requestedDays);

        var request = new LeaveRequest(
            dto.EmployeeId, dto.LeaveTypeId, dto.StartDate, dto.EndDate, requestedDays, dto.Reason);

        await _unitOfWork.LeaveRequests.AddAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return LeaveRequestMapper.ToDto(request);
    }
}