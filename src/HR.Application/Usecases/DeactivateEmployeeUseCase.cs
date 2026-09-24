using HR.Application.Exceptions;
using HR.Application.Interfaces;
using HR.Domain.Entities;

namespace HR.Application.UseCases;

public class DeactivateEmployeeUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateEmployeeUseCase(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task ExecuteAsync(Guid employeeId)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId)
            ?? throw new NotFoundException(nameof(Employee), employeeId);

        employee.Deactivate();
        await _unitOfWork.SaveChangesAsync();
    }
}