using HR.Application.DTOs;
using HR.Application.Interfaces;
using HR.Application.Mapping;
using HR.Domain.Entities;

namespace HR.Application.UseCases;

public class OnboardEmployeeUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public OnboardEmployeeUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeDto> ExecuteAsync(CreateEmployeeDto dto)
    {
        // Uniqueness check — Email is required unique per Section 3.2.
        var existing = await _unitOfWork.Employees.GetByEmailAsync(dto.Email);
        if (existing is not null)
            throw new InvalidOperationException($"An employee with email '{dto.Email}' already exists.");

        var employee = new Employee(
            dto.Name, dto.Email, dto.Department, dto.DateJoined, dto.Role, dto.PasswordHash, dto.ManagerId);

        await _unitOfWork.Employees.AddAsync(employee);

        // FR3/US6: auto-initialize balances for the current year, pro-rated by join date.
        var leaveTypes = await _unitOfWork.LeaveTypes.GetAllAsync();
        var currentYear = DateTime.UtcNow.Year;

        foreach (var leaveType in leaveTypes)
        {
            var proRatedDays = CalculateProRatedAllocation(leaveType.DefaultDaysPerYear, dto.DateJoined, currentYear);
            var balance = new LeaveBalance(employee.EmployeeId, leaveType.LeaveTypeId, currentYear, proRatedDays);
            await _unitOfWork.LeaveBalances.AddAsync(balance);
        }

        await _unitOfWork.SaveChangesAsync();

        return EmployeeMapper.ToDto(employee);
    }

    // Pro-rates the annual allocation by the fraction of the year remaining from the
    // join date. An employee joining exactly on Jan 1 gets the full allocation;
    // someone joining July 1 gets roughly half, rounded to 2 decimal places to match
    // LeaveBalance.RemainingDays' decimal(5,2) precision.
    private static decimal CalculateProRatedAllocation(int defaultDaysPerYear, DateOnly dateJoined, int currentYear)
    {
        if (dateJoined.Year > currentYear)
            return 0; // Joining in a future year — no allocation yet for this year.

        if (dateJoined.Year < currentYear)
            return defaultDaysPerYear; // Existing employee, full allocation for this year.

        var yearStart = new DateOnly(currentYear, 1, 1);
        var yearEnd = new DateOnly(currentYear, 12, 31);
        var totalDaysInYear = yearEnd.DayNumber - yearStart.DayNumber + 1;
        var remainingDays = yearEnd.DayNumber - dateJoined.DayNumber + 1;

        var proRated = defaultDaysPerYear * ((decimal)remainingDays / totalDaysInYear);
        return Math.Round(proRated, 2);
    }
}