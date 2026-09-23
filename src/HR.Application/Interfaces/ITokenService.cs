using HR.Domain.Entities;

namespace HR.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(Employee employee);
}