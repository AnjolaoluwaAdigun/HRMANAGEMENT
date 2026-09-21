using HR.Domain.Enums;

namespace HR.Domain.Entities;

public class LeaveRequest
{
    public Guid LeaveRequestId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid LeaveTypeId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public decimal RequestedDays { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public RequestStatus Status { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public string? ApprovalComment { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private LeaveRequest() { }

    public LeaveRequest(
        Guid employeeId,
        Guid leaveTypeId,
        DateOnly startDate,
        DateOnly endDate,
        decimal requestedDays,
        string reason)
    {
        if (endDate < startDate)
            throw new ArgumentException("EndDate cannot be before StartDate.", nameof(endDate));
        if (requestedDays <= 0)
            throw new ArgumentException("RequestedDays must be positive.", nameof(requestedDays));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required.", nameof(reason));

        LeaveRequestId = Guid.NewGuid();
        EmployeeId = employeeId;
        LeaveTypeId = leaveTypeId;
        StartDate = startDate;
        EndDate = endDate;
        RequestedDays = requestedDays;
        Reason = reason;
        Status = RequestStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    // FR6: an employee cannot submit overlapping requests for the same date range.
    // Standard half-open interval overlap check: [StartDate, EndDate] ranges overlap
    // unless one ends before the other starts.
    public bool OverlapsWith(DateOnly otherStart, DateOnly otherEnd)
    {
        return StartDate <= otherEnd && otherStart <= EndDate;
    }

    public void Approve(Guid approverId, string? comment = null)
    {
        EnsurePending();
        Status = RequestStatus.Approved;
        ApprovedBy = approverId;
        ApprovalComment = comment;
    }

    public void Reject(Guid approverId, string? comment = null)
    {
        EnsurePending();
        Status = RequestStatus.Denied;
        ApprovedBy = approverId;
        ApprovalComment = comment;
    }

    // FR7: only pending requests can be cancelled.
    public void Cancel()
    {
        if (Status != RequestStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot cancel a request with status '{Status}'. Only Pending requests can be cancelled.");

        Status = RequestStatus.Cancelled;
    }

    private void EnsurePending()
    {
        if (Status != RequestStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot act on a request with status '{Status}'. Only Pending requests can be approved or rejected.");
    }
}