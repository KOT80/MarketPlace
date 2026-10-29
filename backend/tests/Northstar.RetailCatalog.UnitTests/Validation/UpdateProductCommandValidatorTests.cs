using Northstar.RetailCatalog.Application.Products.Commands.UpdateProduct;
using Northstar.RetailCatalog.Application.Products.DTOs;
using Northstar.RetailCatalog.Domain.Entities;
using Xunit;

namespace Northstar.RetailCatalog.UnitTests.Validation;

public class UpdateProductCommandValidatorTests
{
    private static UpdateProductCommand Command(
        string sku = "SKU-001",
        string name = "Товар",
        string? description = null,
        Guid? categoryId = null) =>
        new(Guid.NewGuid(), new UpdateProductInputDto(sku, name, description, categoryId ?? Guid.NewGuid()));

    [Fact]
    public void ValidCommand_HasNoFailures()
    {
        var validator = new UpdateProductCommandValidator();

        var result = validator.Validate(Command());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void WhitespaceSku_Invalid()
    {
        var validator = new UpdateProductCommandValidator();

        var result = validator.Validate(Command(sku: "   "));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void NameTooLong_Invalid()
    {
        var validator = new UpdateProductCommandValidator();

        var result = validator.Validate(Command(name: new string('N', Product.MaxNameLength + 1)));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void DescriptionTooLong_Invalid()
    {
        var validator = new UpdateProductCommandValidator();

        var result = validator.Validate(Command(description: new string('D', Product.MaxDescriptionLength + 1)));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void DescriptionNull_IsValid()
    {
        var validator = new UpdateProductCommandValidator();

        var result = validator.Validate(Command(description: null));

        Assert.True(result.IsValid);
    }
}
