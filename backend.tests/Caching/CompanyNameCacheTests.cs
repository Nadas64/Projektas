using backend.Caching;

namespace Backend.Tests.Caching;

public class CompanyNameCacheTests
{
    [Fact]
    public void TryGet_WhenSymbolIsMissing_ReturnsFalse()
    {
        var cache = new CompanyNameCache();

        Assert.False(cache.TryGet("AAPL", out var companyName));
        Assert.Null(companyName);
    }

    [Fact]
    public void TryAdd_ThenTryGet_IsCaseInsensitive()
    {
        var cache = new CompanyNameCache();

        Assert.True(cache.TryAdd("AAPL", "Apple Inc."));
        Assert.True(cache.TryGet("aapl", out var companyName));
        Assert.Equal("Apple Inc.", companyName);
    }

    [Fact]
    public void TryAdd_WhenSymbolAlreadyExists_DoesNotOverwrite()
    {
        var cache = new CompanyNameCache();
        cache.TryAdd("AAPL", "Apple Inc.");

        Assert.False(cache.TryAdd("AAPL", "Another name"));
        Assert.True(cache.TryGet("AAPL", out var companyName));
        Assert.Equal("Apple Inc.", companyName);
    }

    [Fact]
    public void ConcurrentAdds_DoNotLoseEntries()
    {
        var cache = new CompanyNameCache();

        Parallel.For(0, 1_000, i =>
            Assert.True(cache.TryAdd($"SYM{i}", $"Company {i}")));

        Parallel.For(0, 1_000, i =>
        {
            Assert.True(cache.TryGet($"SYM{i}", out var companyName));
            Assert.Equal($"Company {i}", companyName);
        });
    }

    [Fact]
    public void ConcurrentAdds_ForSameSymbol_OnlyOneWins()
    {
        var cache = new CompanyNameCache();
        var successes = 0;

        Parallel.For(0, 100, i =>
        {
            if (cache.TryAdd("AAPL", $"Company {i}"))
            {
                Interlocked.Increment(ref successes);
            }
        });

        Assert.Equal(1, successes);
    }
}
