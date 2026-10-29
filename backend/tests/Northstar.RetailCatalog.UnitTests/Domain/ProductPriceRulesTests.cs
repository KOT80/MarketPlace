using Northstar.RetailCatalog.Domain.Entities;
using Northstar.RetailCatalog.Domain.Exceptions;
using Xunit;

namespace Northstar.RetailCatalog.UnitTests.Domain;

public class ProductPriceRulesTests
{
    private static Product NewProduct(decimal price = 100m) => Product.Create(
        "SKU-001", "Товар", "Описание", Guid.NewGuid(), price, DateTime.UtcNow);

    [Fact]
    public void Create_WithNegativePrice_Throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            Product.Create("SKU-001", "Товар", null, Guid.NewGuid(), -0.01m, DateTime.UtcNow));
    }

    [Fact]
    public void Create_DoesNotTouchHistory()
    {
        var product = NewProduct(150m);

        Assert.Empty(product.PriceHistory);
    }

    [Fact]
    public void ChangePrice_ActualChange_ReturnsHistoryAndUpdatesPrice()
    {
        var product = NewProduct(100m);
        var changedAt = DateTime.UtcNow;

        var history = product.ChangePrice(150m, changedAt);

        Assert.NotNull(history);
        Assert.Equal(100m, history!.OldPrice);
        Assert.Equal(150m, history.NewPrice);
        Assert.Equal(product.Id, history.ProductId);
        Assert.Equal(changedAt, history.ChangedAtUtc);
        Assert.Equal(150m, product.Price);
        Assert.Equal(changedAt, product.UpdatedAtUtc);
    }

    [Fact]
    public void ChangePrice_SameValue_IsNoOpWithoutHistory()
    {
        var product = NewProduct(100m);

        var history = product.ChangePrice(100m, DateTime.UtcNow);

        Assert.Null(history);
        Assert.Equal(100m, product.Price);
        Assert.Null(product.UpdatedAtUtc);
    }

    [Fact]
    public void ChangePrice_ToZero_IsAllowed()
    {
        var product = NewProduct(100m);

        var history = product.ChangePrice(0m, DateTime.UtcNow);

        Assert.NotNull(history);
        Assert.Equal(0m, product.Price);
    }

    [Fact]
    public void ChangePrice_Negative_Throws()
    {
        var product = NewProduct(100m);

        Assert.Throws<DomainRuleException>(() => product.ChangePrice(-1m, DateTime.UtcNow));
        Assert.Equal(100m, product.Price);
    }
}
