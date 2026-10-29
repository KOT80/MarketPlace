using Northstar.RetailCatalog.Domain.Common;
using Northstar.RetailCatalog.Domain.Exceptions;

namespace Northstar.RetailCatalog.Domain.Entities;

public class Product
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 4000;

    public const decimal MaxPrice = 999_999_999_999.99m;

    public Guid Id { get; set; }

    public string Sku { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public Guid CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<ProductPriceHistory> PriceHistory { get; set; } = new List<ProductPriceHistory>();

    public static Product Create(
        string rawSku,
        string name,
        string? description,
        Guid categoryId,
        decimal price,
        DateTime createdAtUtc)
    {
        if (price < 0m)
        {
            throw new DomainRuleException("Цена не может быть отрицательной.");
        }

        return new Product
        {
            Id = Guid.NewGuid(),
            Sku = SkuNormalizer.Normalize(rawSku),
            Name = name,
            Description = description,
            CategoryId = categoryId,
            Price = price,
            IsActive = true,
            CreatedAtUtc = createdAtUtc,
        };
    }

    public ProductPriceHistory? ChangePrice(decimal newPrice, DateTime changedAtUtc)
    {
        if (newPrice < 0m)
        {
            throw new DomainRuleException("Цена не может быть отрицательной.");
        }

        if (newPrice == Price)
        {
            return null;
        }

        var entry = new ProductPriceHistory
        {
            Id = Guid.NewGuid(),
            ProductId = Id,
            OldPrice = Price,
            NewPrice = newPrice,
            ChangedAtUtc = changedAtUtc,
        };

        Price = newPrice;
        UpdatedAtUtc = changedAtUtc;
        return entry;
    }

    public void Archive(DateTime updatedAtUtc)
    {
        if (!IsActive)
        {
            throw new StateConflictException("Товар уже архивирован.");
        }

        IsActive = false;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Restore(DateTime updatedAtUtc)
    {
        if (IsActive)
        {
            throw new StateConflictException("Товар не архивирован.");
        }

        IsActive = true;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void UpdateDetails(
        string rawSku,
        string name,
        string? description,
        Guid categoryId,
        DateTime updatedAtUtc)
    {
        Sku = SkuNormalizer.Normalize(rawSku);
        Name = name;
        Description = description;
        CategoryId = categoryId;
        UpdatedAtUtc = updatedAtUtc;
    }
}
