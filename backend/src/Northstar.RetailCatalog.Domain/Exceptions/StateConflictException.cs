namespace Northstar.RetailCatalog.Domain.Exceptions;

public class StateConflictException : Exception
{
    public StateConflictException(string message)
        : base(message)
    {
    }
}
