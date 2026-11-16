using System.Net;
using System.Text.Json;
using backend.Caching;
using backend.Dtos;
using backend.Gateways.Implementations;

namespace Backend.Tests.Services;

public class FinnhubGatewayTests
{
    private static FinnhubGateway CreateService(string responseJson, CompanyNameCache? cache = null)
        => CreateService(new StubHttpMessageHandler(responseJson), cache);

    private static FinnhubGateway CreateService(HttpMessageHandler handler, CompanyNameCache? cache = null)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://finnhub.io/api/v1/")
        };
        return new FinnhubGateway(httpClient, cache ?? new CompanyNameCache());
    }

    [Fact]
    public async Task SearchSymbolsAsync_DuplicateSymbols_KeepsShortestDescription()
    {
        const string responseJson = """
            {
                "count": 3,
                "result": [
                    { "description": "APPLE INC (LONG FORM)", "displaySymbol": "AAPL", "symbol": "AAPL", "type": "Common Stock" },
                    { "description": "APPLE INC", "displaySymbol": "AAPL", "symbol": "AAPL", "type": "Common Stock" },
                    { "description": "MICROSOFT CORP", "displaySymbol": "MSFT", "symbol": "MSFT", "type": "Common Stock" }
                ]
            }
            """;
        var service = CreateService(responseJson);

        var result = await service.SearchSymbolsAsync("app");

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(["AAPL", "MSFT"], result.Result.Select(r => r.Symbol));
        Assert.Equal("APPLE INC", result.Result.Single(r => r.Symbol == "AAPL").Description);
    }

    [Fact]
    public async Task SearchSymbolsAsync_WhenFinnhubReturnsAnErrorBody_DoesNotThrow()
    {
        // FinnHub error responses (e.g. bad/missing API key) omit "result" entirely,
        // which is exactly the shape the null-reference guard protects against.
        const string responseJson = """{ "error": "You don't have access to this resource." }""";
        var service = CreateService(responseJson);

        var result = await service.SearchSymbolsAsync("app");

        Assert.NotNull(result);
        Assert.Null(result.Result);
    }

    [Fact]
    public async Task GetQuoteAsync_ValidQuote_ReturnsParsedQuote()
    {
        const string responseJson = """{ "c": 150.5, "pc": 148.25, "t": 1767225600 }""";
        var service = CreateService(responseJson);

        var quote = await service.GetQuoteAsync("AAPL", default);

        Assert.Equal(new StockQuote(150.5m, 148.25m, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)), quote);
    }

    [Fact]
    public async Task GetCompanyNameAsync_WhenCalledAgain_UsesCachedName()
    {
        const string responseJson = """{ "name": "Apple Inc." }""";
        var handler = new StubHttpMessageHandler(responseJson);
        var service = CreateService(handler);

        var first = await service.GetCompanyNameAsync("AAPL", default);
        var second = await service.GetCompanyNameAsync("aapl", default);

        Assert.Equal("Apple Inc.", first);
        Assert.Equal("Apple Inc.", second);
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task GetCompanyNameAsync_WhenNameIsMissing_DoesNotCacheFailure()
    {
        const string responseJson = """{ "name": "" }""";
        var handler = new StubHttpMessageHandler(responseJson);
        var service = CreateService(handler);

        var first = await service.GetCompanyNameAsync("AAPL", default);
        var second = await service.GetCompanyNameAsync("AAPL", default);

        Assert.Null(first);
        Assert.Null(second);
        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task GetCompanyNewsAsync_MoreThanTenArticles_ReturnsNewestTen()
    {
        var articles = Enumerable.Range(1, 12).Select(i => new NewsItem($"Headline {i}", "", "", "", i));
        var service = CreateService(JsonSerializer.Serialize(articles));

        var news = await service.GetCompanyNewsAsync("AAPL", default);

        Assert.Equal(Enumerable.Range(3, 10).Reverse().Select(i => (long)i), news.Select(n => n.Datetime));
    }

    private sealed class StubHttpMessageHandler(string responseJson) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            };
            return Task.FromResult(response);
        }
    }
}
