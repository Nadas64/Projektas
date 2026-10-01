using backend.Data.Repositories;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;

namespace backend.Services;

public class PortfolioService(
    IUnitOfWork uow,
    IUserRepository users,
    IPortfolioRepository portfolios,
    IHoldingRepository holdings,
    IStockRepository stocks,
    ITransactionRepository transactions,
    FinnhubService finnhub)
{
    private const string DefaultUsername = "default";
    private const decimal StartingCash = 10000m;

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
        if (symbol.Length == 0)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "A stock symbol is required.");
        }

        var marketIsOpen = await finnhub.IsMarketOpenAsync(cancellationToken);
        if (!marketIsOpen)
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
        var defaultPortfolio = await GetOrCreateDefaultPortfolioAsync(cancellationToken);

        await uow.BeginTransactionAsync(cancellationToken);

        var portfolio = await portfolios.GetByIdForUpdateAsync(defaultPortfolio.Id, cancellationToken);
        if (portfolio is null)
        {
            throw new AppException(StatusCodes.Status404NotFound, "Portfolio record was not found.");
        }

        var holding = await holdings.GetByPortfolioAndSymbolAsync(
            portfolio.Id,
            symbol,
            cancellationToken);
        var stock = await stocks.GetBySymbolAsync(symbol, cancellationToken);
        var total = price.Value * request.Quantity;

        if (request.Type == TradeType.Buy)
        {
            if (portfolio.Cash < total)
            {
                throw new AppException(StatusCodes.Status400BadRequest, "Insufficient cash for this purchase.");
            }

            if (stock is null)
            {
                stock = new Stock { Symbol = symbol, CompanyName = symbol };
                await stocks.AddAsync(stock, cancellationToken);
            }

            if (holding is null)
            {
                holding = new Holding
                {
                    Portfolio = portfolio,
                    Stock = stock,
                    Quantity = request.Quantity,
                    AverageBuyPrice = price.Value
                };
                await holdings.AddAsync(holding, cancellationToken);
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
                holdings.Remove(holding);
            }

            portfolio.Cash += total;
        }

        await transactions.AddAsync(new Transaction
        {
            Portfolio = portfolio,
            Stock = stock,
            Type = request.Type.ToString().ToUpperInvariant(),
            Quantity = request.Quantity,
            Price = price.Value,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);
        await uow.CommitAsync(cancellationToken);

        return await CreatePortfolioResponseAsync(portfolio);
    }

    private async Task<Portfolio> GetOrCreateDefaultPortfolioAsync(CancellationToken ct = default)
    {
        var user = await users.GetByUsernameWithPortfolioAsync(DefaultUsername, ct);

        if (user is null)
        {
            user = new User
            {
                Username = DefaultUsername,
                Password = "",
                Portfolio = new Portfolio { Cash = StartingCash }
            };
            await users.AddAsync(user, ct);
            await uow.SaveChangesAsync(ct);
        }
        else if (user.Portfolio is null)
        {
            user.Portfolio = new Portfolio { Cash = StartingCash };
            await uow.SaveChangesAsync(ct);
        }

        return user.Portfolio;
    }

    private async Task<PortfolioResponse> CreatePortfolioResponseAsync(Portfolio portfolio)
    {
        var items = await holdings.GetByPortfolioAsync(portfolio.Id);

        return new PortfolioResponse(
            portfolio.Cash,
            [.. items.Select(item => new HoldingResponse(item.Stock.Symbol, item.Quantity))]);
    }
}
