namespace HR.Domain.Exceptions;

public class InsufficientBalanceException : DomainException
{
    public InsufficientBalanceException(decimal remaining, decimal requested)
        : base($"Insufficient leave balance: {remaining} day(s) remaining, {requested} day(s) requested.")
    {
    }
}