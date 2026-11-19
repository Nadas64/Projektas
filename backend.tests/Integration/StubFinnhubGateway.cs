using backend.Dtos;
using backend.Gateways.Interfaces;

namespace Backend.Tests.Integration;

public sealed class StubFinnhubGateway : IFinnhubGateway
{
    public decimal? Price { get; set; }
    public decimal? PreviousClose { get; set; }
    public string? CompanyName { get; set; } = "Apple Inc.";

    public Task<StockQuote?> GetQuoteAsync(string symbol, CancellationToken cancellationToken)
        => Task.FromResult(Price is null
            ? null
            : new StockQuote(Price.Value, PreviousClose ?? Price.Value, DateTime.UtcNow));

    public Task<decimal?> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken)
        => Task.FromResult(Price);

    public Task<string?> GetCompanyNameAsync(string symbol, CancellationToken cancellationToken)
        => Task.FromResult(CompanyName);

    public Task<bool> IsMarketOpenAsync(CancellationToken cancellationToken)
        => Task.FromResult(true);

    public Task<SymbolSearchResponse?> SearchSymbolsAsync(string query)
        => Task.FromResult<SymbolSearchResponse?>(null);

    public Task<IReadOnlyList<NewsItem>> GetCompanyNewsAsync(
        string symbol,
        CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<NewsItem>>([]);
}
