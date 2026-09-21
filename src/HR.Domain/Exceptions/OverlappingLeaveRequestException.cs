namespace HR.Domain.Exceptions;

public class OverlappingLeaveRequestException : DomainException
{
    public OverlappingLeaveRequestException()
        : base("An overlapping leave request already exists for this date range.")
    {
    }
}