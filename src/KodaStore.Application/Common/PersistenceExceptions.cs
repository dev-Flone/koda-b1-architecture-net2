namespace KodaStore.Application.Common;

public sealed class UniqueConstraintViolationException : Exception
{
    public UniqueConstraintViolationException(string message, Exception innerException) : base(message, innerException)
    {

    }
}

public sealed class DataConcurrencyException : Exception
{
    public DataConcurrencyException(string message, Exception innerException)
    : base(message, innerException) { }
}