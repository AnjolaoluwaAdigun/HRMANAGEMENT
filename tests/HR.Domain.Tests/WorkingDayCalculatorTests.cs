using HR.Domain.Services;
using Xunit;

namespace HR.Domain.Tests;

public class WorkingDayCalculatorTests
{
    [Fact]
    public void CalculateWorkingDays_SingleWeekday_ReturnsOne()
    {
        var monday = new DateOnly(2026, 9, 21);

        var result = WorkingDayCalculator.CalculateWorkingDays(monday, monday);

        Assert.Equal(1, result);
    }

    [Fact]
    public void CalculateWorkingDays_FullWeek_ExcludesWeekend()
    {
        // Monday 21st through Sunday 27th Sep 2026 = 5 weekdays, 2 weekend days
        var monday = new DateOnly(2026, 9, 21);
        var sunday = new DateOnly(2026, 9, 27);

        var result = WorkingDayCalculator.CalculateWorkingDays(monday, sunday);

        Assert.Equal(5, result);
    }

    [Fact]
    public void CalculateWorkingDays_EntirelyWithinWeekend_ReturnsZero()
    {
        var saturday = new DateOnly(2026, 9, 26);
        var sunday = new DateOnly(2026, 9, 27);

        var result = WorkingDayCalculator.CalculateWorkingDays(saturday, sunday);

        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateWorkingDays_EndBeforeStart_Throws()
    {
        var start = new DateOnly(2026, 9, 21);
        var end = new DateOnly(2026, 9, 20);

        Assert.Throws<ArgumentException>(() =>
            WorkingDayCalculator.CalculateWorkingDays(start, end));
    }
}