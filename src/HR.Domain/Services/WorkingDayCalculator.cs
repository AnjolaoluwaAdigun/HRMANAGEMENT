namespace HR.Domain.Services;

// FR5: calculates requested days excluding weekends.
// Public holiday exclusion is an explicit stretch goal per Section 1.3 — not implemented here,
// but this is the single place you'd extend if you add it later.
public static class WorkingDayCalculator
{
    public static decimal CalculateWorkingDays(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("EndDate cannot be before StartDate.", nameof(endDate));

        var count = 0;
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                count++;
        }

        return count;
    }
}