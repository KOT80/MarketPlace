namespace Northstar.RetailCatalog.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }
}
