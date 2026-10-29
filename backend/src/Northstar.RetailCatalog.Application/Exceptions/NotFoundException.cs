namespace Northstar.RetailCatalog.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string resourceName, object key)
        : base($"\"{resourceName}\" с идентификатором {key} не найден.")
    {
    }
}
