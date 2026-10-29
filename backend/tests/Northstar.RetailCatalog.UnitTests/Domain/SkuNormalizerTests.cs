using Northstar.RetailCatalog.Domain.Common;
using Xunit;

namespace Northstar.RetailCatalog.UnitTests.Domain;

public class SkuNormalizerTests
{
    [Theory]
    [InlineData(" abc-123 ", "ABC-123")]
    [InlineData("abc  123", "ABC 123")]
    [InlineData("ABC-123", "ABC-123")]
    [InlineData("ÿ-Товар", "Ÿ-ТОВАР")]
    [InlineData(null, "")]
    [InlineData("   ", "")]
    [InlineData("", "")]
    public void Normalize_ProducesExpectedValue(string? input, string expected)
    {
        Assert.Equal(expected, SkuNormalizer.Normalize(input));
    }

    [Fact]
    public void Normalize_ComparesCaseInsensitive()
    {
        Assert.Equal(SkuNormalizer.Normalize("sku-001"), SkuNormalizer.Normalize("SKU-001"));
    }
}
