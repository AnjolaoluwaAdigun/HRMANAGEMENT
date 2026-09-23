using HR.Application.DTOs;
using HR.Application.Interfaces;
using HR.Application.Mapping;
using HR.Domain.Entities;

namespace HR.Application.UseCases;

public class OnboardEmployeeUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public OnboardEmployeeUseCase(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<EmployeeDto> ExecuteAsync(CreateEmployeeDto dto)
    {
        var existing = await _unitOfWork.Employees.GetByEmailAsync(dto.Email);
        if (existing is not null)
            throw new InvalidOperationException($"An employee with email '{dto.Email}' already exists.");

        var passwordHash = _passwordHasher.Hash(dto.Password);

        var employee = new Employee(
            dto.Name, dto.Email, dto.Department, dto.DateJoined, dto.Role, passwordHash, dto.ManagerId);

        await _unitOfWork.Employees.AddAsync(employee);

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

    private static decimal CalculateProRatedAllocation(int defaultDaysPerYear, DateOnly dateJoined, int currentYear)
    {
        if (dateJoined.Year > currentYear) return 0;
        if (dateJoined.Year < currentYear) return defaultDaysPerYear;

        var yearStart = new DateOnly(currentYear, 1, 1);
        var yearEnd = new DateOnly(currentYear, 12, 31);
        var totalDaysInYear = yearEnd.DayNumber - yearStart.DayNumber + 1;
        var remainingDays = yearEnd.DayNumber - dateJoined.DayNumber + 1;

        return Math.Round(defaultDaysPerYear * ((decimal)remainingDays / totalDaysInYear), 2);
    }
}