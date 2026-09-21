using HR.Domain.Exceptions;

namespace HR.Domain.Entities;

public class LeaveBalance
{
    public Guid LeaveBalanceId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid LeaveTypeId { get; private set; }
    public int Year { get; private set; }
    public decimal RemainingDays { get; private set; }

    // Used by EF Core for optimistic concurrency control, so two concurrent
    // approvals can't both read-modify-write the same row and push it negative.
    public byte[]? RowVersion { get; private set; }

    private LeaveBalance() { }

    public LeaveBalance(Guid employeeId, Guid leaveTypeId, int year, decimal initialDays)
    {
        if (initialDays < 0)
            throw new ArgumentException("Initial balance cannot be negative.", nameof(initialDays));

        LeaveBalanceId = Guid.NewGuid();
        EmployeeId = employeeId;
        LeaveTypeId = leaveTypeId;
        Year = year;
        RemainingDays = initialDays;
    }

    // Core business rule: a balance must never go negative, including
    // under concurrent approval attempts (NFR-Data Integrity).
    public void Deduct(decimal days)
    {
        if (days <= 0)
            throw new ArgumentException("Days to deduct must be positive.", nameof(days));

        if (RemainingDays - days < 0)
            throw new InsufficientBalanceException(RemainingDays, days);

        RemainingDays -= days;
    }

    // Used when a previously-approved request is reversed (not required by MVP,
    // but keeps the balance logic symmetric and easy to extend).
    public void Restore(decimal days)
    {
        if (days <= 0)
            throw new ArgumentException("Days to restore must be positive.", nameof(days));

        RemainingDays += days;
    }

    public bool HasSufficientBalance(decimal days) => RemainingDays >= days;
}