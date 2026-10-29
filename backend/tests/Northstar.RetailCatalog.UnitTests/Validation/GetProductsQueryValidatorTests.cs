using Northstar.RetailCatalog.Application.Common;
using Northstar.RetailCatalog.Application.Products.Queries.GetProducts;
using Xunit;

namespace Northstar.RetailCatalog.UnitTests.Validation;

public class GetProductsQueryValidatorTests
{
    private static GetProductsQuery Query(
        int page = 1,
        int pageSize = 20,
        string sortBy = ProductSortFields.CreatedAt,
        string sortDirection = "desc") =>
        new(new ProductQueryRequest
        {
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDirection = sortDirection,
        });

    [Fact]
    public void Defaults_AreValid()
    {
        var validator = new GetProductsQueryValidator();

        var result = validator.Validate(Query());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PageBelowOne_Invalid(int page)
    {
        var validator = new GetProductsQueryValidator();

        var result = validator.Validate(Query(page: page));

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(101)]
    public void PageSizeOutOfRange_Invalid(int pageSize)
    {
        var validator = new GetProductsQueryValidator();

        var result = validator.Validate(Query(pageSize: pageSize));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void PageSizeAtMaximum_IsValid()
    {
        var validator = new GetProductsQueryValidator();

        var result = validator.Validate(Query(pageSize: ProductQueryRequest.MaxPageSize));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("name")]
    [InlineData("sku")]
    [InlineData("price")]
    [InlineData("createdAt")]
    [InlineData("CREATEDAT")]
    public void AllowedSortBy_IsValid(string sortBy)
    {
        var validator = new GetProductsQueryValidator();

        var result = validator.Validate(Query(sortBy: sortBy));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("password")]
    [InlineData("category_id")]
    [InlineData("unknown")]
    public void DisallowedSortBy_Invalid(string sortBy)
    {
        var validator = new GetProductsQueryValidator();

        var result = validator.Validate(Query(sortBy: sortBy));

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("asc")]
    [InlineData("desc")]
    [InlineData("ASC")]
    public void AllowedSortDirection_IsValid(string sortDirection)
    {
        var validator = new GetProductsQueryValidator();

        var result = validator.Validate(Query(sortDirection: sortDirection));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("up")]
    [InlineData("")]
    public void DisallowedSortDirection_Invalid(string sortDirection)
    {
        var validator = new GetProductsQueryValidator();

        var result = validator.Validate(Query(sortDirection: sortDirection));

        Assert.False(result.IsValid);
    }
}
