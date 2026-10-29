using Northstar.RetailCatalog.Domain.Entities;
using Northstar.RetailCatalog.Domain.Exceptions;
using Xunit;

namespace Northstar.RetailCatalog.UnitTests.Domain;

public class ProductStateRulesTests
{
    private static Product NewActiveProduct() => Product.Create(
        "SKU-001", "Товар", null, Guid.NewGuid(), 100m, DateTime.UtcNow);

    [Fact]
    public void Create_IsActiveByDefault()
    {
        Assert.True(NewActiveProduct().IsActive);
    }

    [Fact]
    public void Archive_DeactivatesAndStampsUpdate()
    {
        var product = NewActiveProduct();
        var at = DateTime.UtcNow;

        product.Archive(at);

        Assert.False(product.IsActive);
        Assert.Equal(at, product.UpdatedAtUtc);
    }

    [Fact]
    public void Archive_AlreadyArchived_ThrowsStateConflict()
    {
        var product = NewActiveProduct();
        product.Archive(DateTime.UtcNow);

        Assert.Throws<StateConflictException>(() => product.Archive(DateTime.UtcNow));
    }

    [Fact]
    public void Restore_ArchivedProduct_Activates()
    {
        var product = NewActiveProduct();
        product.Archive(DateTime.UtcNow);
        var at = DateTime.UtcNow;

        product.Restore(at);

        Assert.True(product.IsActive);
        Assert.Equal(at, product.UpdatedAtUtc);
    }

    [Fact]
    public void Restore_NotArchived_ThrowsStateConflict()
    {
        var product = NewActiveProduct();

        Assert.Throws<StateConflictException>(() => product.Restore(DateTime.UtcNow));
    }

    [Fact]
    public void Archive_KeepsPriceHistory()
    {
        var product = NewActiveProduct();
        var history = product.ChangePrice(200m, DateTime.UtcNow);
        Assert.NotNull(history);
        product.PriceHistory.Add(history!);

        product.Archive(DateTime.UtcNow);

        Assert.Single(product.PriceHistory);
    }
}
