using HR.Application.Interfaces;

namespace HR.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string plainTextPassword) =>
        BCrypt.Net.BCrypt.HashPassword(plainTextPassword);

    public bool Verify(string plainTextPassword, string hash) =>
        BCrypt.Net.BCrypt.Verify(plainTextPassword, hash);
}