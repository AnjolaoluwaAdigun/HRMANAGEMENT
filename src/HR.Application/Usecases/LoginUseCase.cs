using HR.Application.DTOs;
using HR.Application.Interfaces;
using HR.Application.Mapping;

namespace HR.Application.UseCases;

public class LoginUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginUseCase(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDto?> ExecuteAsync(LoginDto dto)
    {
        var employee = await _unitOfWork.Employees.GetByEmailAsync(dto.Email);

        // Deliberately vague on failure — don't reveal whether the email exists.
        if (employee is null || !employee.IsActive || !_passwordHasher.Verify(dto.Password, employee.PasswordHash))
            return null;

        var token = _tokenService.GenerateToken(employee);
        return new AuthResultDto(token, EmployeeMapper.ToDto(employee));
    }
}