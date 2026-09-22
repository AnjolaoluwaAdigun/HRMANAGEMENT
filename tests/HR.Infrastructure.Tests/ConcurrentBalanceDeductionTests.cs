using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Infrastructure.Tests;

// Proves NFR-Data Integrity in practice: two concurrent approval attempts against
// the same LeaveBalance must not both succeed. Uses a real SQL Server instance
// (not mocks) because optimistic concurrency via RowVersion only exists at the
// database level — mocked repositories can't simulate a real race condition.
public class ConcurrentBalanceDeductionTests
{
    // Points at the same running SQL Server container used for `dotnet ef database update`.
    private const string ConnectionString =
        "Server=localhost,1433;Database=HRManagementPlatform;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";

    private static DbContextOptions<ApplicationDbContext> Options =>
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

    [Fact]
    public async Task ConcurrentApprovals_OnlyOneSucceeds_BalanceNeverGoesNegative()
    {
        // Arrange: seed one employee, one leave type, and a balance with exactly
        // enough days for ONE of two requests, not both. If both approvals
        // succeeded, the balance would go negative — which must never happen.
        var employeeId = Guid.NewGuid();
        var leaveTypeId = Guid.NewGuid();
        var year = 2026;

        await using (var seedContext = new ApplicationDbContext(Options))
        {
            var employee = new Employee(
                "Concurrency Test", $"concurrency-{Guid.NewGuid()}@test.com", "Engineering",
                new DateOnly(2024, 1, 1), Role.Employee, "hash");
            // Reflection-free way to force a known Id isn't available (private setters),
            // so we capture the Id the constructor actually generated.
            employeeId = employee.EmployeeId;

            var leaveType = new LeaveType("Annual", 20);
            leaveTypeId = leaveType.LeaveTypeId;

            // Exactly 5 days available — each of the two requests below asks for 5.
            var balance = new LeaveBalance(employeeId, leaveTypeId, year, initialDays: 5);

            seedContext.Employees.Add(employee);
            seedContext.LeaveTypes.Add(leaveType);
            seedContext.LeaveBalances.Add(balance);
            await seedContext.SaveChangesAsync();
        }

        // Act: two separate DbContext instances (simulating two separate concurrent
        // requests/threads) each load the SAME balance row, then both attempt to
        // deduct 5 days and save. Only one should win; the other must fail cleanly.
        async Task<bool> AttemptDeduction()
        {
            await using var context = new ApplicationDbContext(Options);
            var balance = await context.LeaveBalances
                .FirstAsync(b => b.EmployeeId == employeeId && b.LeaveTypeId == leaveTypeId && b.Year == year);

            balance.Deduct(5);

            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Expected for the loser of the race: its RowVersion is stale
                // because the other transaction already updated the row.
                return false;
            }
        }

        var results = await Task.WhenAll(AttemptDeduction(), AttemptDeduction());

        // Assert: exactly one of the two attempts succeeded.
        Assert.Single(results, r => r);
        Assert.Single(results, r => !r);

        // Assert: the final balance reflects exactly ONE deduction, never negative,
        // never double-deducted.
        await using var verifyContext = new ApplicationDbContext(Options);
        var finalBalance = await verifyContext.LeaveBalances
            .FirstAsync(b => b.EmployeeId == employeeId && b.LeaveTypeId == leaveTypeId && b.Year == year);

        Assert.Equal(0, finalBalance.RemainingDays);
        Assert.True(finalBalance.RemainingDays >= 0);
    }
}