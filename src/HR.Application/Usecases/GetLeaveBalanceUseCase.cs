using HR.Application.DTOs;
using HR.Application.Interfaces;

namespace HR.Application.UseCases;

public class GetLeaveBalanceUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public GetLeaveBalanceUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<LeaveBalanceDto>> ExecuteAsync(Guid employeeId, int year)
    {
        var balances = await _unitOfWork.LeaveBalances.GetForEmployeeAsync(employeeId, year);

        var result = new List<LeaveBalanceDto>();
        foreach (var balance in balances)
        {
            var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(balance.LeaveTypeId);
            result.Add(new LeaveBalanceDto(
                balance.LeaveTypeId,
                leaveType?.Name ?? "Unknown",
                balance.Year,
                balance.RemainingDays));
        }

        return result;
    }
}