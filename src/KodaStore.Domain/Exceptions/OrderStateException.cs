namespace KodaStore.Domain.Exceptions;

public class OrderStateException : InvalidOperationException
{
    public OrderStateException(string message) : base(message)
    {

    }
}
