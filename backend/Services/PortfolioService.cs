using backend.Comparers;
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

    public async Task<PortfolioResponse> GetPortfolioAsync(CancellationToken cancellationToken = default)
    {
        var portfolio = await GetOrCreateDefaultPortfolioAsync(cancellationToken);
        return (await CreatePortfolioResponseAsync(portfolio, cancellationToken)).Response;
    }

    public async Task<IReadOnlyList<PortfolioHistoryPoint>> GetHistoryAsync(
        CancellationToken cancellationToken = default)
    {
        var portfolio = await GetOrCreateDefaultPortfolioAsync(cancellationToken);
        var trades = (await transactions.GetByPortfolioAsync(portfolio.Id, cancellationToken))
            .Where(t => t.PortfolioValueAfter is not null)
            .OrderBy(t => t.CreatedAt)
            .ToList();

        var points = new List<PortfolioHistoryPoint>();
        if (trades.Count > 0)
        {
            points.Add(new(trades[0].CreatedAt.AddSeconds(-1), StartingCash));
        }
        points.AddRange(trades.Select(t => new PortfolioHistoryPoint(t.CreatedAt, t.PortfolioValueAfter!.Value)));

        var (current, _) = await CreatePortfolioResponseAsync(portfolio, cancellationToken);
        points.Add(new(DateTime.UtcNow, current.TotalBalance));

        return points;
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

        // var marketIsOpen = await finnhub.IsMarketOpenAsync(cancellationToken);
        // if (!marketIsOpen)
        // {
        //     throw new AppException(StatusCodes.Status409Conflict, "Trading is unavailable while the market is closed.");
        // }

        var price = await finnhub.GetCurrentPriceAsync(symbol, cancellationToken);
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
                var companyName = await finnhub.GetCompanyNameAsync(symbol, cancellationToken);
                stock = new Stock { Symbol = symbol, CompanyName = companyName ?? symbol };
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

        var transaction = new Transaction
        {
            Portfolio = portfolio,
            Stock = stock,
            Type = request.Type,
            Quantity = request.Quantity,
            Price = price.Value,
            CreatedAt = DateTime.UtcNow
        };
        await transactions.AddAsync(transaction, cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);
        await uow.CommitAsync(cancellationToken);

        var (response, allPricesLoaded) = await CreatePortfolioResponseAsync(portfolio, cancellationToken);
        if (allPricesLoaded)
        {
            transaction.PortfolioValueAfter = response.TotalBalance;
            await uow.SaveChangesAsync(cancellationToken);
        }

        return response;
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

    private async Task<(PortfolioResponse Response, bool AllPricesLoaded)> CreatePortfolioResponseAsync(
        Portfolio portfolio,
        CancellationToken cancellationToken = default)
    {
        var items = await holdings.GetByPortfolioAsync(portfolio.Id, cancellationToken);
        var trades = await transactions.GetByPortfolioAsync(portfolio.Id, cancellationToken);

        var quotes = await Task.WhenAll(
            items.Select(item => finnhub.GetQuoteAsync(item.Stock.Symbol, cancellationToken)));

        var holdingResponses = new List<HoldingResponse>();
        decimal holdingsValue = 0;
        decimal todaysGain = 0;
        decimal totalCost = 0;

        foreach (var (item, quote) in items.Zip(quotes))
        {
            var currentPrice = quote?.Current ?? item.AverageBuyPrice;

            var cost = item.AverageBuyPrice * item.Quantity;
            var currentValue = currentPrice * item.Quantity;
            var gainDollars = currentValue - cost;

            holdingsValue += currentValue;
            todaysGain += CalculateTodaysGain(item.Stock.Symbol, item.Quantity, quote, trades);
            totalCost += cost;

            holdingResponses.Add(new HoldingResponse(
                item.Stock.Symbol,
                item.Stock.CompanyName,
                item.Quantity,
                item.AverageBuyPrice,
                currentValue,
                Percent(gainDollars, cost),
                gainDollars));
        }

        var heldSymbols = items.Select(i => i.Stock.Symbol).ToHashSet();
        var soldOutSymbols = trades
            .Where(t => t.CreatedAt >= DateTime.UtcNow.AddDays(-5) && !heldSymbols.Contains(t.Stock.Symbol))
            .Select(t => t.Stock.Symbol)
            .Distinct()
            .ToList();

        var soldOutQuotes = await Task.WhenAll(
            soldOutSymbols.Select(s => finnhub.GetQuoteAsync(s, cancellationToken)));

        foreach (var (symbol, quote) in soldOutSymbols.Zip(soldOutQuotes))
        {
            todaysGain += CalculateTodaysGain(symbol, 0, quote, trades);
        }


        holdingResponses.Sort(new HoldingValueComparer());

        var totalBalance = portfolio.Cash + holdingsValue;
        var totalGains = totalBalance - StartingCash;

        var response = new PortfolioResponse(
            totalBalance,
            portfolio.Cash,
            todaysGain,
            Percent(todaysGain, totalBalance - todaysGain),
            totalGains,
            Percent(totalGains, StartingCash),
            holdingResponses);

        return (response, quotes.All(q => q is not null));
    }

    private static decimal Percent(decimal part, decimal whole)
    {
        return whole == 0 ? 0 : Math.Round(part / whole * 100, 2);
    }

    private static decimal CalculateTodaysGain(string symbol, int quantity, StockQuote? quote, List<Transaction> trades)
    {
        if (quote is null)
        {
            return 0;
        }

        var gain = (quote.Current - quote.PreviousClose) * quantity;

        var tradesSinceClose = trades.Where(t =>
            t.Stock.Symbol == symbol
            && t.CreatedAt.Date >= quote.UpdatedAt.Date);

        foreach (var trade in tradesSinceClose)
        {
            var moveBeforeTrade = (trade.Price - quote.PreviousClose) * trade.Quantity;
            gain += trade.Type == TradeType.Buy ? -moveBeforeTrade : moveBeforeTrade;
        }

        return gain;
    }
}