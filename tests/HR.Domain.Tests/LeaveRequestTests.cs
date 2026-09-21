using HR.Domain.Entities;
using HR.Domain.Enums;
using Xunit;

namespace HR.Domain.Tests;

public class LeaveRequestTests
{
    private static LeaveRequest CreateRequest(DateOnly start, DateOnly end) =>
        new(Guid.NewGuid(), Guid.NewGuid(), start, end, requestedDays: 3, reason: "Vacation");

    // --- Overlap detection (FR6) ---

    [Fact]
    public void OverlapsWith_IdenticalRange_ReturnsTrue()
    {
        var request = CreateRequest(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5));

        Assert.True(request.OverlapsWith(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5)));
    }

    [Fact]
    public void OverlapsWith_PartialOverlap_ReturnsTrue()
    {
        var request = CreateRequest(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 10));

        // New request starts mid-way through the existing one.
        Assert.True(request.OverlapsWith(new DateOnly(2026, 10, 8), new DateOnly(2026, 10, 15)));
    }

    [Fact]
    public void OverlapsWith_OneDayTouchingBoundary_ReturnsTrue()
    {
        var request = CreateRequest(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5));

        // New request starts exactly on the existing request's end date.
        Assert.True(request.OverlapsWith(new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 8)));
    }

    [Fact]
    public void OverlapsWith_NonOverlappingRange_ReturnsFalse()
    {
        var request = CreateRequest(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5));

        Assert.False(request.OverlapsWith(new DateOnly(2026, 10, 6), new DateOnly(2026, 10, 10)));
    }

    [Fact]
    public void OverlapsWith_DistantNonOverlappingRange_ReturnsFalse()
    {
        var request = CreateRequest(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5));

        Assert.False(request.OverlapsWith(new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 5)));
    }

    // --- State transitions ---

    [Fact]
    public void NewRequest_HasStatusPending()
    {
        var request = CreateRequest(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5));

        Assert.Equal(RequestStatus.Pending, request.Status);
    }

    [Fact]
    public void Approve_PendingRequest_SetsStatusApprovedAndRecordsApprover()
    {
        var request = CreateRequest(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5));
        var managerId = Guid.NewGuid();

        request.Approve(managerId, "Enjoy your trip");

        Assert.Equal(RequestStatus.Approved, request.Status);
        Assert.Equal(managerId, request.ApprovedBy);
        Assert.Equal("Enjoy your trip", request.ApprovalComment);
    }

    [Fact]
    public void Reject_PendingRequest_SetsStatusRejected()
    {
        var request = CreateRequest(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5));

        request.Reject(Guid.NewGuid(), "Team is short-staffed that week");

        Assert.Equal(RequestStatus.Denied, request.Status);
    }

    [Fact]
    public void Approve_AlreadyApprovedRequest_Throws()
    {
        var request = CreateRequest(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5));
        request.Approve(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => request.Approve(Guid.NewGuid()));
    }

    // --- Cancellation (FR7) ---

    [Fact]
    public void Cancel_PendingRequest_SetsStatusCancelled()
    {
        var request = CreateRequest(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5));

        request.Cancel();

        Assert.Equal(RequestStatus.Cancelled, request.Status);
    }

    [Fact]
    public void Cancel_ApprovedRequest_Throws()
    {
        var request = CreateRequest(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5));
        request.Approve(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => request.Cancel());
    }

    // --- Validation ---

    [Fact]
    public void Constructor_EndDateBeforeStartDate_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new LeaveRequest(
                Guid.NewGuid(), Guid.NewGuid(),
                new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 1),
                requestedDays: 3, reason: "Invalid range"));
    }

    [Fact]
    public void Constructor_EmptyReason_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new LeaveRequest(
                Guid.NewGuid(), Guid.NewGuid(),
                new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5),
                requestedDays: 3, reason: ""));
    }
}