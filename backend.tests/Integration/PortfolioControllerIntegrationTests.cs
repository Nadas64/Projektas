using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using backend.Dtos;
using backend.Enums;

namespace Backend.Tests.Integration;

public class PortfolioControllerIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task Trade_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var factory = new PortfolioApiFactory();
        using var client = factory.CreateClient();

        var response = await PostTradeAsync(client, "AAPL", "BUY", 1);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Trade_BuyNewStock_UpdatesPortfolioAndTradeHistory()
    {
        using var factory = new PortfolioApiFactory();
        factory.Finnhub.Price = 100m;
        using var client = await CreateAuthenticatedClientAsync(factory);

        var response = await PostTradeAsync(client, " aapl ", "BUY", 3);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var portfolio = await response.Content.ReadFromJsonAsync<PortfolioResponse>(JsonOptions);
        Assert.NotNull(portfolio);
        Assert.Equal(9700m, portfolio.BuyingPower);
        Assert.Equal(10000m, portfolio.TotalBalance);

        var holding = Assert.Single(portfolio.Holdings);
        Assert.Equal("AAPL", holding.Symbol);
        Assert.Equal("Apple Inc.", holding.CompanyName);
        Assert.Equal(3, holding.Quantity);
        Assert.Equal(100m, holding.AverageBuyPrice);
        Assert.Equal(300m, holding.CurrentValue);

        var trades = await client.GetFromJsonAsync<List<TradeHistoryItem>>("/api/portfolio/trades", JsonOptions);
        var trade = Assert.Single(trades!);
        Assert.Equal("AAPL", trade.Symbol);
        Assert.Equal(TradeType.Buy, trade.Type);
        Assert.Equal(3, trade.Quantity);
        Assert.Equal(100m, trade.Price);
        Assert.Equal(300m, trade.Total);
    }

    [Fact]
    public async Task Trade_BuyWithInsufficientCash_ReturnsBadRequest()
    {
        using var factory = new PortfolioApiFactory();
        factory.Finnhub.Price = 5000m;
        using var client = await CreateAuthenticatedClientAsync(factory);

        var response = await PostTradeAsync(client, "AAPL", "BUY", 3);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "Insufficient cash for this purchase.",
            await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Trade_SellMoreSharesThanHeld_ReturnsBadRequest()
    {
        using var factory = new PortfolioApiFactory();
        factory.Finnhub.Price = 100m;
        using var client = await CreateAuthenticatedClientAsync(factory);

        var buy = await PostTradeAsync(client, "AAPL", "BUY", 1);
        Assert.Equal(HttpStatusCode.OK, buy.StatusCode);

        var response = await PostTradeAsync(client, "AAPL", "SELL", 2);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "Not enough shares are available to sell.",
            await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Trade_SellAllShares_RemovesHoldingAndCreditsCash()
    {
        using var factory = new PortfolioApiFactory();
        factory.Finnhub.Price = 100m;
        using var client = await CreateAuthenticatedClientAsync(factory);
        Assert.Equal(HttpStatusCode.OK, (await PostTradeAsync(client, "AAPL", "BUY", 5)).StatusCode);

        factory.Finnhub.Price = 120m;
        var response = await PostTradeAsync(client, "AAPL", "SELL", 5);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var portfolio = await response.Content.ReadFromJsonAsync<PortfolioResponse>(JsonOptions);
        Assert.NotNull(portfolio);
        Assert.Empty(portfolio.Holdings);
        Assert.Equal(10100m, portfolio.BuyingPower);
        Assert.Equal(10100m, portfolio.TotalBalance);
    }

    [Fact]
    public async Task Trade_WhenPriceIsUnavailable_ReturnsBadGateway()
    {
        using var factory = new PortfolioApiFactory();
        factory.Finnhub.Price = null;
        using var client = await CreateAuthenticatedClientAsync(factory);

        var response = await PostTradeAsync(client, "AAPL", "BUY", 1);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Equal(
            "Could not retrieve a current price for this stock.",
            await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Trade_WithNonPositiveQuantity_ReturnsBadRequest()
    {
        using var factory = new PortfolioApiFactory();
        factory.Finnhub.Price = 100m;
        using var client = await CreateAuthenticatedClientAsync(factory);

        var response = await PostTradeAsync(client, "AAPL", "BUY", 0);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "Quantity must be greater than zero.",
            await response.Content.ReadAsStringAsync());
    }

    private static async Task<HttpClient> CreateAuthenticatedClientAsync(
        PortfolioApiFactory factory,
        string username = "alice")
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(username, "Secret123"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return client;
    }

    private static Task<HttpResponseMessage> PostTradeAsync(
        HttpClient client,
        string symbol,
        string type,
        int quantity)
    {
        return client.PostAsJsonAsync("/api/trade", new
        {
            symbol,
            type,
            quantity
        });
    }
}
