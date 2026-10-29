using Northstar.RetailCatalog.Domain.Entities;
using Xunit;

namespace Northstar.RetailCatalog.UnitTests;

public class ProductTests
{
    [Fact]
    public void NewProduct_IsActiveByDefault()
    {
        var product = new Product();

        Assert.True(product.IsActive);
    }

    [Fact]
    public void NewProduct_HasNullOptionalDescription()
    {
        var product = new Product { Description = null };

        Assert.Null(product.Description);
    }
}
