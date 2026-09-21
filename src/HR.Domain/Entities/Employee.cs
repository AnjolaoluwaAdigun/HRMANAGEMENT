using HR.Domain.Enums;

namespace HR.Domain.Entities;

public class Employee
{
    public Guid EmployeeId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;
    public Guid? ManagerId { get; private set; }
    public DateOnly DateJoined { get; private set; }
    public Role Role { get; private set; }
    public EmployeeStatus Status { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;

    // EF Core needs a parameterless constructor; keep it private so it's
    // only usable by the ORM, not by application code.
    private Employee() { }

    public Employee(
        string name,
        string email,
        string department,
        DateOnly dateJoined,
        Role role,
        string passwordHash,
        Guid? managerId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(department))
            throw new ArgumentException("Department is required.", nameof(department));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("PasswordHash is required.", nameof(passwordHash));

        EmployeeId = Guid.NewGuid();
        Name = name;
        Email = email;
        Department = department;
        DateJoined = dateJoined;
        Role = role;
        PasswordHash = passwordHash;
        ManagerId = managerId;
        Status = EmployeeStatus.Active;
    }

    public void UpdateDetails(string name, string email, string department, Guid? managerId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(department))
            throw new ArgumentException("Department is required.", nameof(department));

        // Prevent an employee from being made their own manager.
        if (managerId.HasValue && managerId.Value == EmployeeId)
            throw new ArgumentException("An employee cannot be their own manager.", nameof(managerId));

        Name = name;
        Email = email;
        Department = department;
        ManagerId = managerId;
    }

    public void Deactivate() => Status = EmployeeStatus.Inactive;

    public void Reactivate() => Status = EmployeeStatus.Active;

    public bool IsActive => Status == EmployeeStatus.Active;

    public bool IsDirectManagerOf(Employee other) =>
        other.ManagerId.HasValue && other.ManagerId.Value == EmployeeId;
}