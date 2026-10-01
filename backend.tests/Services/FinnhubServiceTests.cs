using System.Net;
using backend.Services;

namespace Backend.Tests.Services;

public class FinnhubServiceTests
{
    private static FinnhubService CreateService(string responseJson)
    {
        var handler = new StubHttpMessageHandler(responseJson);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://finnhub.io/api/v1/")
        };
        return new FinnhubService(httpClient);
    }

    [Fact]
    public async Task SearchSymbolsAsync_DedupesResultsBySymbol_KeepingShortestDescription()
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

    private sealed class StubHttpMessageHandler(string responseJson) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            };
            return Task.FromResult(response);
        }
    }
}
