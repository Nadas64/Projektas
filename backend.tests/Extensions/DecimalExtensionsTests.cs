using backend.Extensions;

namespace Backend.Tests.Extensions;

public class DecimalExtensionsTests
{
    [Theory]
    [InlineData(15.15, 100)]
    [InlineData(10, 10)]
    [InlineData(99, 287)]
    public async Task PercentageOfIsCorrect(decimal part, decimal whole)
    {
        Assert.Equal(Math.Round(part / whole * 100, 2), part.PercentOf(whole));
    }

    [Fact]
    public async Task DivisionByZero_IsZero()
    {
        Assert.Equal(0, 12m.PercentOf(0m));
    }
}
