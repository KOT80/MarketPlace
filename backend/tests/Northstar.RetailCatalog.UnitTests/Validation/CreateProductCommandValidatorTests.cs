using Northstar.RetailCatalog.Application.Products.Commands.CreateProduct;
using Northstar.RetailCatalog.Application.Products.DTOs;
using Northstar.RetailCatalog.Domain.Entities;
using Xunit;

namespace Northstar.RetailCatalog.UnitTests.Validation;

public class CreateProductCommandValidatorTests
{
    private const string Prefix = "Input.";

    private static CreateProductCommand Command(
        string sku = "SKU-001",
        string name = "Товар",
        string? description = null,
        Guid? categoryId = null,
        decimal price = 10m) =>
        new(new CreateProductInputDto(sku, name, description, categoryId ?? Guid.NewGuid(), price));

    private static void AssertErrorOnField(
        FluentValidation.Results.ValidationResult result, string fieldName) =>
        Assert.Contains(result.Errors, e => e.PropertyName == Prefix + fieldName);

    [Fact]
    public void ValidCommand_HasNoFailures()
    {
        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(Command());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void EmptyOrWhitespaceSku_Invalid(string sku)
    {
        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(Command(sku: sku));

        Assert.False(result.IsValid);
        AssertErrorOnField(result, nameof(CreateProductInputDto.Sku));
    }

    [Fact]
    public void SkuExceedingMaxLength_Invalid()
    {
        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(Command(sku: new string('A', 65)));

        Assert.False(result.IsValid);
        AssertErrorOnField(result, nameof(CreateProductInputDto.Sku));
    }

    [Fact]
    public void SkuNormalizedFitsMaxLength_WhenRawLongerButSpacesTrimmed_IsValid()
    {
        var validator = new CreateProductCommandValidator();
        var raw = $"  {new string('A', 64)}  ";

        var result = validator.Validate(Command(sku: raw));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void NameTooLong_Invalid()
    {
        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(Command(name: new string('N', Product.MaxNameLength + 1)));

        Assert.False(result.IsValid);
        AssertErrorOnField(result, nameof(CreateProductInputDto.Name));
    }

    [Fact]
    public void NameEmpty_Invalid()
    {
        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(Command(name: ""));

        Assert.False(result.IsValid);
        AssertErrorOnField(result, nameof(CreateProductInputDto.Name));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100.0)]
    public void NegativePrice_Invalid(double price)
    {
        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(Command(price: (decimal)price));

        Assert.False(result.IsValid);
        AssertErrorOnField(result, nameof(CreateProductInputDto.Price));
    }

    [Theory]
    [InlineData(10.001)]
    [InlineData(10.999)]
    public void PriceWithMoreThanTwoDecimals_Invalid(double price)
    {
        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(Command(price: (decimal)price));

        Assert.False(result.IsValid);
        AssertErrorOnField(result, nameof(CreateProductInputDto.Price));
    }

    [Fact]
    public void ZeroPrice_IsValid()
    {
        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(Command(price: 0m));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void EmptyCategory_Invalid()
    {
        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(Command(categoryId: Guid.Empty));

        Assert.False(result.IsValid);
        AssertErrorOnField(result, nameof(CreateProductInputDto.CategoryId));
    }

    [Fact]
    public void DescriptionTooLong_Invalid()
    {
        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(Command(description: new string('D', Product.MaxDescriptionLength + 1)));

        Assert.False(result.IsValid);
        AssertErrorOnField(result, nameof(CreateProductInputDto.Description));
    }

    [Fact]
    public void DescriptionNull_IsValid()
    {
        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(Command(description: null));

        Assert.True(result.IsValid);
    }
}
