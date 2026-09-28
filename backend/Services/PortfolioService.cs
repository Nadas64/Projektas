using backend.Data;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class PortfolioService(
    AppDbContext db,
    FinnhubService finnhub)
{
    private const string DefaultUsername = "default";
    private const decimal StartingCash = 1000m;
    private readonly AppDbContext _db = db;

    public async Task<PortfolioResponse> GetPortfolioAsync()
    {
        var portfolio = await GetOrCreateDefaultPortfolioAsync();
        return await CreatePortfolioResponseAsync(portfolio);
    }

    public async Task<PortfolioResponse> TradeAsync(
        TradeRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Quantity must be greater than zero.");
        }

        var symbol = request.Symbol.Trim().ToUpperInvariant();
        var type = request.Type.Trim().ToUpperInvariant();
        if (symbol.Length == 0 || (type != "BUY" && type != "SELL"))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "A stock symbol and trade type BUY or SELL are required.");
        }

        var marketIsOpen = await finnhub.IsMarketOpenAsync(cancellationToken);
        if (marketIsOpen is null)
        {
            throw new AppException(StatusCodes.Status502BadGateway, "Could not verify whether the market is open.");
        }

        if (!marketIsOpen.Value)
        {
            throw new AppException(StatusCodes.Status409Conflict, "Trading is unavailable while the market is closed.");
        }

        var price = await finnhub.GetCurrentPriceAsync(symbol, cancellationToken); ;
        if (price is null)
        {
            throw new AppException(StatusCodes.Status502BadGateway, "Could not retrieve a current price for this stock.");
        }

        // TODO: Remade logic after registration and users system creation
        // FIXME
        var defaultPortfolio = await GetOrCreateDefaultPortfolioAsync();
        await using var dbTransaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var portfolioId = defaultPortfolio.Id;
        _db.Entry(defaultPortfolio).State = EntityState.Detached;
        var portfolio = await _db.Portfolios
            .FromSqlInterpolated($"SELECT * FROM \"Portfolios\" WHERE \"Id\" = {portfolioId} FOR UPDATE")
            .SingleAsync(cancellationToken);

        var holding = await _db.Holdings
            .SingleOrDefaultAsync(
                item => item.PortfolioId == portfolio.Id && item.Stock.Symbol == symbol,
                cancellationToken);
        var stock = await _db.Stocks.SingleOrDefaultAsync(
            item => item.Symbol == symbol,
            cancellationToken);
        var total = price.Value * request.Quantity;

        if (type == "BUY")
        {
            if (portfolio.Cash < total)
            {
                throw new AppException(StatusCodes.Status400BadRequest, "Insufficient cash for this purchase.");

            }

            if (stock is null)
            {
                stock = new Stock { Symbol = symbol, CompanyName = symbol };
                _db.Stocks.Add(stock);
            }

            if (holding is null)
            {
                holding = new Holding
                {
                    PortfolioId = portfolio.Id,
                    Stock = stock,
                    Quantity = request.Quantity,
                    AverageBuyPrice = price.Value
                };
                _db.Holdings.Add(holding);
            }
            else
            {
                var updatedQuantity = holding.Quantity + request.Quantity;
                holding.AverageBuyPrice =
                    (holding.AverageBuyPrice * holding.Quantity + total) / updatedQuantity;
                holding.Quantity = updatedQuantity;
            }

            portfolio.Cash -= total;
        }
        else
        {
            if (holding is null || holding.Quantity < request.Quantity)
            {
                throw new AppException(StatusCodes.Status400BadRequest, "Not enough shares are available to sell.");
            }

            if (stock is null)
            {
                throw new AppException(StatusCodes.Status404NotFound, "Stock record for this holding was not found.");
            }

            holding.Quantity -= request.Quantity;
            if (holding.Quantity == 0)
            {
                _db.Holdings.Remove(holding);
            }

            portfolio.Cash += total;
        }

        _db.Transactions.Add(new Transaction
        {
            PortfolioId = portfolio.Id,
            Stock = stock,
            Type = type,
            Quantity = request.Quantity,
            Price = price.Value,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
        await dbTransaction.CommitAsync(cancellationToken);

        return await CreatePortfolioResponseAsync(portfolio);
    }

    private async Task<Portfolio> GetOrCreateDefaultPortfolioAsync()
    {
        var user = await _db.Users
            .Include(item => item.Portfolio)
            .SingleOrDefaultAsync(item => item.Username == DefaultUsername);

        if (user is null)
        {
            user = new User
            {
                Username = DefaultUsername,
                Password = "",
                Portfolio = new Portfolio { Cash = StartingCash }
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
        else if (user.Portfolio is null)
        {
            user.Portfolio = new Portfolio { Cash = StartingCash };
            await _db.SaveChangesAsync();
        }

        return user.Portfolio;
    }

    private async Task<PortfolioResponse> CreatePortfolioResponseAsync(Portfolio portfolio)
    {
        var holdings = await _db.Holdings
            .Where(item => item.PortfolioId == portfolio.Id)
            .OrderBy(item => item.Stock.Symbol)
            .Select(item => new HoldingResponse(item.Stock.Symbol, item.Quantity))
            .ToListAsync();

        return new PortfolioResponse(portfolio.Cash, holdings);
    }


}