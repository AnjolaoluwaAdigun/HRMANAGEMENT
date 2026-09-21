namespace HR.Domain.Entities;

public class LeaveType
{
    public Guid LeaveTypeId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int DefaultDaysPerYear { get; private set; }

    private LeaveType() { }

    public LeaveType(string name, int defaultDaysPerYear)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (defaultDaysPerYear < 0)
            throw new ArgumentException("DefaultDaysPerYear cannot be negative.", nameof(defaultDaysPerYear));

        LeaveTypeId = Guid.NewGuid();
        Name = name;
        DefaultDaysPerYear = defaultDaysPerYear;
    }

    public void Update(string name, int defaultDaysPerYear)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (defaultDaysPerYear < 0)
            throw new ArgumentException("DefaultDaysPerYear cannot be negative.", nameof(defaultDaysPerYear));

        Name = name;
        DefaultDaysPerYear = defaultDaysPerYear;
    }
}