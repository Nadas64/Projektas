using backend.Comparers;
using backend.Data.Repositories.Interfaces;
using backend.Dtos;
using backend.Enums;
using backend.Exceptions;
using backend.Gateways.Interfaces;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Backend.Tests.Services;

public class PortfolioServiceTests
{
    private readonly Portfolio _portfolio = new() { Id = 1, Cash = PortfolioService.StartingCash };
    private readonly List<Holding> _heldStocks = [];
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPortfolioRepository _portfolios = Substitute.For<IPortfolioRepository>();
    private readonly IHoldingRepository _holdings = Substitute.For<IHoldingRepository>();
    private readonly IStockRepository _stocks = Substitute.For<IStockRepository>();
    private readonly IFinnhubGateway _finnhub = Substitute.For<IFinnhubGateway>();
    private readonly PortfolioService _service;

    public PortfolioServiceTests()
    {
        _users.GetByUsernameWithPortfolioAsync("alice").Returns(new User { Username = "alice", Portfolio = _portfolio });
        _portfolios.GetByIdAsync(_portfolio.Id).Returns(_portfolio);
        var transactions = Substitute.For<ITransactionRepository>();
        transactions.GetByPortfolioAsync(_portfolio.Id).Returns(new List<Transaction>());
        _holdings.GetByPortfolioAsync(_portfolio.Id).Returns(_heldStocks);

        _uow.Users.Returns(_users);
        _uow.Portfolios.Returns(_portfolios);
        _uow.Holdings.Returns(_holdings);
        _uow.Stocks.Returns(_stocks);
        _uow.Transactions.Returns(transactions);

        _service = new PortfolioService(_uow, _finnhub, new HoldingValueComparer());
    }

    private Holding AddAaplHolding(int quantity, decimal averageBuyPrice)
    {
        var holding = new Holding
        {
            Portfolio = _portfolio,
            Stock = new Stock { Symbol = "AAPL" },
            Quantity = quantity,
            AverageBuyPrice = averageBuyPrice
        };
        _heldStocks.Add(holding);
        _holdings.GetByPortfolioAndSymbolAsync(_portfolio.Id, "AAPL").Returns(holding);
        _stocks.GetBySymbolAsync("AAPL").Returns(holding.Stock);
        return holding;
    }

    private void SetAaplPrice(decimal price) => _finnhub.GetCurrentPriceAsync("AAPL", default).Returns(price);

    private Task<PortfolioResponse> TradeAsync(TradeType type, int quantity, string symbol = "AAPL")
        => _service.TradeAsync("alice", new TradeRequest(symbol, type, quantity), default);

    [Fact]
    public async Task Buy_NewStock_AddsHoldingAndDeductsCash()
    {
        SetAaplPrice(100m);

        await TradeAsync(TradeType.Buy, 3, symbol: " aapl ");

        await _holdings.Received().AddAsync(Arg.Is<Holding>(h =>
            h.Stock.Symbol == "AAPL" && h.Quantity == 3 && h.AverageBuyPrice == 100m));
        Assert.Equal(9700m, _portfolio.Cash);
    }

    [Fact]
    public async Task Buy_ExistingHolding_UpdatesAverageBuyPrice()
    {
        var holding = AddAaplHolding(quantity: 2, averageBuyPrice: 100m);
        SetAaplPrice(160m);

        await TradeAsync(TradeType.Buy, 2);

        Assert.Equal(4, holding.Quantity);
        Assert.Equal(130m, holding.AverageBuyPrice);
    }

    [Fact]
    public async Task Buy_WithInsufficientCash_Throws()
    {
        SetAaplPrice(5000m);

        await Assert.ThrowsAsync<InsufficientCashException>(() => TradeAsync(TradeType.Buy, 3));

        Assert.Equal(PortfolioService.StartingCash, _portfolio.Cash);
    }

    [Fact]
    public async Task Buy_WithExactlyEnoughCash_SpendsAllCash()
    {
        SetAaplPrice(PortfolioService.StartingCash / 2);

        await TradeAsync(TradeType.Buy, 2);

        Assert.Equal(0m, _portfolio.Cash);
    }

    [Fact]
    public async Task Buy_KnownStockWithoutHolding_ReusesStock()
    {
        var stock = new Stock { Symbol = "AAPL" };
        _stocks.GetBySymbolAsync("AAPL").Returns(stock);
        SetAaplPrice(100m);

        await TradeAsync(TradeType.Buy, 1);

        await _holdings.Received().AddAsync(Arg.Is<Holding>(h => h.Stock == stock && h.Quantity == 1));
        await _stocks.DidNotReceive().AddAsync(Arg.Any<Stock>());
    }

    [Theory]
    [InlineData("Apple Inc.", "Apple Inc.")]
    [InlineData(null, "AAPL")]
    public async Task Buy_NewStock_StoresCompanyNameOrFallsBackToSymbol(string? companyName, string expectedName)
    {
        _finnhub.GetCompanyNameAsync("AAPL", default).Returns(companyName);
        SetAaplPrice(100m);

        await TradeAsync(TradeType.Buy, 1);

        await _stocks.Received().AddAsync(Arg.Is<Stock>(s => s.Symbol == "AAPL" && s.CompanyName == expectedName));
    }

    [Fact]
    public async Task Sell_AllShares_RemovesHoldingAndAddsCash()
    {
        var holding = AddAaplHolding(quantity: 5, averageBuyPrice: 100m);
        SetAaplPrice(120m);

        await TradeAsync(TradeType.Sell, 5);

        _holdings.Received().Remove(holding);
        Assert.Equal(10600m, _portfolio.Cash);
    }

    [Fact]
    public async Task Sell_MoreSharesThanHeld_Throws()
    {
        AddAaplHolding(quantity: 1, averageBuyPrice: 100m);
        SetAaplPrice(100m);

        var ex = await Assert.ThrowsAsync<AppException>(() => TradeAsync(TradeType.Sell, 2));

        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Trade_WithNonPositiveQuantity_Throws(int quantity)
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => TradeAsync(TradeType.Buy, quantity));

        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
    }

    [Fact]
    public async Task Trade_WhenPriceIsUnavailable_Throws()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => TradeAsync(TradeType.Buy, 1));

        Assert.Equal(StatusCodes.Status502BadGateway, ex.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Trade_WithBlankSymbol_Throws(string symbol)
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => TradeAsync(TradeType.Buy, 1, symbol));

        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
    }

    [Fact]
    public async Task Trade_WithUnknownUser_Throws()
    {
        SetAaplPrice(100m);

        var ex = await Assert.ThrowsAsync<AppException>(
            () => _service.TradeAsync("bob", new TradeRequest("AAPL", TradeType.Buy, 1), default));

        Assert.Equal(StatusCodes.Status404NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task Trade_WhenUserHasNoPortfolio_Throws()
    {
        _users.GetByUsernameWithPortfolioAsync("alice").Returns(new User { Username = "alice" });
        SetAaplPrice(100m);

        await Assert.ThrowsAsync<InvalidOperationException>(() => TradeAsync(TradeType.Buy, 1));
    }

    [Fact]
    public async Task Trade_WhenPortfolioRecordIsMissing_Throws()
    {
        _portfolios.GetByIdAsync(_portfolio.Id).Returns((Portfolio?)null);
        SetAaplPrice(100m);

        var ex = await Assert.ThrowsAsync<AppException>(() => TradeAsync(TradeType.Buy, 1));

        Assert.Equal(StatusCodes.Status404NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task GetPortfolioResponse_ValuesHoldingsAtCurrentPrice()
    {
        _portfolio.Cash = 9000m;
        AddAaplHolding(quantity: 10, averageBuyPrice: 100m);
        _finnhub.GetQuoteAsync("AAPL", default).Returns(new StockQuote(150m, 150m, DateTime.UtcNow));

        var response = await _service.GetPortfolioResponseAsync("alice");

        Assert.Equal(10500m, response.TotalBalance);
        Assert.Equal(500m, response.TotalGains);
        Assert.Equal(1500m, Assert.Single(response.Holdings).CurrentValue);
    }
}
