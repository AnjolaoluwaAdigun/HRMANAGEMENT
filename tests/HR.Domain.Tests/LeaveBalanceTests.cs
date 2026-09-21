using HR.Domain.Entities;
using HR.Domain.Exceptions;
using Xunit;

namespace HR.Domain.Tests;

public class LeaveBalanceTests
{
    [Fact]
    public void Deduct_SufficientBalance_ReducesRemainingDays()
    {
        var balance = new LeaveBalance(Guid.NewGuid(), Guid.NewGuid(), 2026, initialDays: 20);

        balance.Deduct(5);

        Assert.Equal(15, balance.RemainingDays);
    }

    [Fact]
    public void Deduct_ExactlyRemainingDays_ReducesToZero()
    {
        var balance = new LeaveBalance(Guid.NewGuid(), Guid.NewGuid(), 2026, initialDays: 5);

        balance.Deduct(5);

        Assert.Equal(0, balance.RemainingDays);
    }

    // This is the core NFR-Data Integrity rule: a balance must NEVER go negative.
    [Fact]
    public void Deduct_MoreThanRemaining_ThrowsInsufficientBalanceException()
    {
        var balance = new LeaveBalance(Guid.NewGuid(), Guid.NewGuid(), 2026, initialDays: 3);

        Assert.Throws<InsufficientBalanceException>(() => balance.Deduct(5));

        // Balance must be unchanged after a failed deduction.
        Assert.Equal(3, balance.RemainingDays);
    }

    [Fact]
    public void Deduct_ZeroOrNegativeDays_ThrowsArgumentException()
    {
        var balance = new LeaveBalance(Guid.NewGuid(), Guid.NewGuid(), 2026, initialDays: 10);

        Assert.Throws<ArgumentException>(() => balance.Deduct(0));
        Assert.Throws<ArgumentException>(() => balance.Deduct(-2));
    }

    [Fact]
    public void HasSufficientBalance_ReturnsTrueWhenEnough()
    {
        var balance = new LeaveBalance(Guid.NewGuid(), Guid.NewGuid(), 2026, initialDays: 10);

        Assert.True(balance.HasSufficientBalance(10));
        Assert.False(balance.HasSufficientBalance(11));
    }

    [Fact]
    public void Restore_AddsDaysBack()
    {
        var balance = new LeaveBalance(Guid.NewGuid(), Guid.NewGuid(), 2026, initialDays: 10);
        balance.Deduct(4);

        balance.Restore(4);

        Assert.Equal(10, balance.RemainingDays);
    }

    [Fact]
    public void Constructor_NegativeInitialDays_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new LeaveBalance(Guid.NewGuid(), Guid.NewGuid(), 2026, initialDays: -1));
    }
}