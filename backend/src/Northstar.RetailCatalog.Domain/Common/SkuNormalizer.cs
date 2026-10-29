namespace Northstar.RetailCatalog.Domain.Common;

public static class SkuNormalizer
{
    public const int MaxLength = 64;

    public static string Normalize(string? rawSku)
    {
        if (string.IsNullOrWhiteSpace(rawSku))
        {
            return string.Empty;
        }

        var collapsed = string.Join(' ', rawSku.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return collapsed.ToUpperInvariant();
    }
}
