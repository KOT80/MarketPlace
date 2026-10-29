namespace Northstar.RetailCatalog.Application.Common;

public static class ProductSortFields
{
    public const string Name = "name";
    public const string Sku = "sku";
    public const string Price = "price";
    public const string CreatedAt = "createdAt";

    public static readonly IReadOnlySet<string> Allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Name,
        Sku,
        Price,
        CreatedAt,
    };
}
