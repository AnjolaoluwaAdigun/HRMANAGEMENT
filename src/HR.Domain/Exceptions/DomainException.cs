namespace HR.Domain.Exceptions;

// Base type for all domain rule violations, so calling layers can catch this
// without depending on the specific rule that was broken.
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}