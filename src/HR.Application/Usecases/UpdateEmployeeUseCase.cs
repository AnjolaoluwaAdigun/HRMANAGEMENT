using HR.Application.DTOs;
using HR.Application.Exceptions;
using HR.Application.Interfaces;
using HR.Application.Mapping;
using HR.Domain.Entities;

namespace HR.Application.UseCases;

public class UpdateEmployeeUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEmployeeUseCase(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<EmployeeDto> ExecuteAsync(Guid employeeId, UpdateEmployeeDto dto)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId)
            ?? throw new NotFoundException(nameof(Employee), employeeId);

        // If the manager is changing, confirm the new manager actually exists.
        if (dto.ManagerId.HasValue)
        {
            var manager = await _unitOfWork.Employees.GetByIdAsync(dto.ManagerId.Value)
                ?? throw new NotFoundException(nameof(Employee), dto.ManagerId.Value);
        }

        employee.UpdateDetails(dto.Name, dto.Email, dto.Department, dto.ManagerId);
        await _unitOfWork.SaveChangesAsync();

        return EmployeeMapper.ToDto(employee);
    }
}