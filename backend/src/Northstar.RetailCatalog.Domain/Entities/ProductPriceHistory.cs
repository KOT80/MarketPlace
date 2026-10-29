namespace Northstar.RetailCatalog.Domain.Entities;

public class ProductPriceHistory
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public decimal OldPrice { get; set; }

    public decimal NewPrice { get; set; }

    public DateTime ChangedAtUtc { get; set; }
}
