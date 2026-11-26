using backend.Data.Repositories.Interfaces;
using backend.Exceptions;
using backend.Gateways.Interfaces;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Backend.Tests.Services;

public class WatchlistServiceTests
{
    private readonly User _user = new() { Id = 1, Username = "alice" };
    private readonly List<WatchlistItem> _watchlistItems = [];
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IStockRepository _stocks = Substitute.For<IStockRepository>();
    private readonly IWatchlistRepository _watchlist = Substitute.For<IWatchlistRepository>();
    private readonly IFinnhubGateway _finnhub = Substitute.For<IFinnhubGateway>();
    private readonly WatchlistService _service;

    public WatchlistServiceTests()
    {
        _users.GetByUsernameAsync("alice").Returns(_user);
        _watchlist.GetByUserAsync(_user.Id).Returns(_watchlistItems);

        _uow.Users.Returns(_users);
        _uow.Stocks.Returns(_stocks);
        _uow.Watchlist.Returns(_watchlist);

        _service = new WatchlistService(_uow, _finnhub);
    }

    [Fact]
    public async Task GetWatchlistAsync_ReturnsItems()
    {
        var item = AddAaplToWatchlist();

        var result = await _service.GetWatchlistAsync("alice");

        var watched = Assert.Single(result);
        Assert.Equal("AAPL", watched.Symbol);
        Assert.Equal("Apple Inc", watched.CompanyName);
        Assert.Equal(item.CreatedAt, watched.AddedAt);
    }

    [Fact]
    public async Task GetWatchlistAsync_UnknownUser_Throws404()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _service.GetWatchlistAsync("bob"));

        Assert.Equal(StatusCodes.Status404NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task AddAsync_NewStock_CreatesStockAndAddsItem()
    {
        _finnhub.GetCurrentPriceAsync("AAPL", default).Returns(150m);
        _finnhub.GetCompanyNameAsync("AAPL", default).Returns("Apple Inc");

        await _service.AddAsync("alice", " aapl ");

        await _stocks.Received().AddAsync(Arg.Is<Stock>(s => s.Symbol == "AAPL" && s.CompanyName == "Apple Inc"));
        await _watchlist.Received().AddAsync(Arg.Is<WatchlistItem>(w =>
            w.User == _user && w.Stock.Symbol == "AAPL"));
        await _uow.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task AddAsync_KnownStock_ReusesStockWithoutCallingFinnhub()
    {
        var stock = new Stock { Symbol = "AAPL", CompanyName = "Apple Inc" };
        _stocks.GetBySymbolAsync("AAPL").Returns(stock);

        await _service.AddAsync("alice", "AAPL");

        await _stocks.DidNotReceive().AddAsync(Arg.Any<Stock>());
        await _finnhub.DidNotReceive().GetCurrentPriceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _watchlist.Received().AddAsync(Arg.Is<WatchlistItem>(w => w.Stock == stock));
    }

    [Fact]
    public async Task AddAsync_UnknownSymbol_Throws404()
    {
        _finnhub.GetCurrentPriceAsync("NOPE", default).Returns((decimal?)null);

        var ex = await Assert.ThrowsAsync<AppException>(() => _service.AddAsync("alice", "NOPE"));

        Assert.Equal(StatusCodes.Status404NotFound, ex.StatusCode);
        await _watchlist.DidNotReceive().AddAsync(Arg.Any<WatchlistItem>());
    }

    [Fact]
    public async Task AddAsync_AlreadyInWatchlist_Throws409()
    {
        AddAaplToWatchlist();

        var ex = await Assert.ThrowsAsync<AppException>(() => _service.AddAsync("alice", "AAPL"));

        Assert.Equal(StatusCodes.Status409Conflict, ex.StatusCode);
        await _watchlist.DidNotReceive().AddAsync(Arg.Any<WatchlistItem>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddAsync_BlankSymbol_Throws400(string symbol)
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _service.AddAsync("alice", symbol));

        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
    }

    [Fact]
    public async Task RemoveAsync_InWatchlist_RemovesItem()
    {
        var item = AddAaplToWatchlist();

        await _service.RemoveAsync("alice", "aapl");

        _watchlist.Received().Remove(item);
        await _uow.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task RemoveAsync_NotInWatchlist_Throws404()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _service.RemoveAsync("alice", "AAPL"));

        Assert.Equal(StatusCodes.Status404NotFound, ex.StatusCode);
        _watchlist.DidNotReceive().Remove(Arg.Any<WatchlistItem>());
    }

    private WatchlistItem AddAaplToWatchlist()
    {
        var item = new WatchlistItem
        {
            User = _user,
            Stock = new Stock { Symbol = "AAPL", CompanyName = "Apple Inc" }
        };
        _watchlistItems.Add(item);
        _watchlist.GetByUserAndSymbolAsync(_user.Id, "AAPL").Returns(item);
        return item;
    }
}
