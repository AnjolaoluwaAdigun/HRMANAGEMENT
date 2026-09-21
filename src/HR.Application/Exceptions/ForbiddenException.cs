namespace HR.Application.Exceptions;

// Thrown when an authenticated user attempts an action outside their permitted
// scope — e.g. a manager approving a request from someone who isn't their direct
// report (US2's 403 rule). The API layer maps this to HTTP 403.
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}