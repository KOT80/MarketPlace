namespace Northstar.RetailCatalog.Application.Exceptions;

public class SkuConflictException : Exception
{
    public SkuConflictException(string normalizedSku)
        : base($"Товар с артикулом \"{normalizedSku}\" уже существует.")
    {
        Sku = normalizedSku;
    }

    public string Sku { get; }
}
