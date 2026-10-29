using Northstar.RetailCatalog.Application.Products.Commands.ChangePrice;
using Northstar.RetailCatalog.Domain.Entities;
using Xunit;

namespace Northstar.RetailCatalog.UnitTests.Validation;

public class ChangePriceCommandValidatorTests
{
    [Theory]
    [InlineData(10.0)]
    [InlineData(0.0)]
    [InlineData(123.45)]
    public void ValidPrices(double price)
    {
        var validator = new ChangePriceCommandValidator();

        var result = validator.Validate(new ChangePriceCommand(Guid.NewGuid(), (decimal)price));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void PriceAtBusinessMax_IsValid()
    {
        var validator = new ChangePriceCommandValidator();

        var result = validator.Validate(new ChangePriceCommand(Guid.NewGuid(), Product.MaxPrice));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void MissingPrice_Invalid()
    {
        var validator = new ChangePriceCommandValidator();

        var result = validator.Validate(new ChangePriceCommand(Guid.NewGuid(), null));

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-5.0)]
    public void NegativePrice_Invalid(double price)
    {
        var validator = new ChangePriceCommandValidator();

        var result = validator.Validate(new ChangePriceCommand(Guid.NewGuid(), (decimal)price));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void TooPrecisePrice_Invalid()
    {
        var validator = new ChangePriceCommandValidator();

        var result = validator.Validate(new ChangePriceCommand(Guid.NewGuid(), 1.234m));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void PriceAboveBusinessMax_Invalid()
    {
        var validator = new ChangePriceCommandValidator();

        var result = validator.Validate(new ChangePriceCommand(Guid.NewGuid(), Product.MaxPrice + 1m));

        Assert.False(result.IsValid);
    }
}
