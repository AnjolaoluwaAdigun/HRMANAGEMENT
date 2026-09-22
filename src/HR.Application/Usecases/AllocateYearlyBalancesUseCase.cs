using HR.Application.Interfaces;
using HR.Domain.Entities;

namespace HR.Application.UseCases;

public class AllocateYearlyBalancesUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public AllocateYearlyBalancesUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(int year)
    {
        var employees = await _unitOfWork.Employees.GetAllAsync();
        var leaveTypes = await _unitOfWork.LeaveTypes.GetAllAsync();

        foreach (var employee in employees.Where(e => e.IsActive))
        {
            foreach (var leaveType in leaveTypes)
            {
                // Skip if a balance already exists for this employee/type/year
                // (unique constraint per Section 3.2 — don't double-allocate).
                var existing = await _unitOfWork.LeaveBalances.GetAsync(
                    employee.EmployeeId, leaveType.LeaveTypeId, year);

                if (existing is null)
                {
                    var balance = new LeaveBalance(
                        employee.EmployeeId, leaveType.LeaveTypeId, year, leaveType.DefaultDaysPerYear);
                    await _unitOfWork.LeaveBalances.AddAsync(balance);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }
}