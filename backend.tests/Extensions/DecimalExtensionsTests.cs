using backend.Extensions;

namespace Backend.Tests.Extensions;

public class DecimalExtensionsTests
{
    [Theory]
    [InlineData(15.15, 100)]
    [InlineData(10, 10)]
    [InlineData(99, 287)]
    public void PercentOf_NonZeroWhole_ReturnsRoundedPercentage(decimal part, decimal whole)
    {
        Assert.Equal(Math.Round(part / whole * 100, 2), part.PercentOf(whole));
    }

    [Fact]
    public void PercentOf_ZeroWhole_ReturnsZero()
    {
        Assert.Equal(0, 12m.PercentOf(0m));
    }
}
