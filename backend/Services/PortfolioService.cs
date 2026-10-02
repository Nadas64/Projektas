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
        return await CreatePortfolioResponseAsync(portfolio, cancellationToken);
    }

    public async Task<IReadOnlyList<PortfolioHistoryPoint>> GetHistoryAsync(
        CancellationToken cancellationToken = default)
    {
        var portfolio = await GetOrCreateDefaultPortfolioAsync(cancellationToken);
        var trades = (await transactions.GetByPortfolioAsync(portfolio.Id, cancellationToken))
            .OrderBy(t => t.CreatedAt)
            .ToList();

        var current = await CreatePortfolioResponseAsync(portfolio, cancellationToken);
        var points = new List<PortfolioHistoryPoint>();

        // Undo all trades to get the cash before the first one
        var cash = portfolio.Cash - trades.Sum(t => CashChange(t));
        var quantities = new Dictionary<string, int>();
        var lastPrices = new Dictionary<string, decimal>();

        foreach (var trade in trades)
        {
            var symbol = trade.Stock.Symbol;
            var quantityChange = trade.Type == "BUY" ? trade.Quantity : -trade.Quantity;

            quantities[symbol] = quantities.GetValueOrDefault(symbol) + quantityChange;
            lastPrices[symbol] = trade.Price;
            cash += CashChange(trade);

            // Between trades we only know each stock's last traded price
            var holdingsValue = quantities.Sum(q => q.Value * lastPrices[q.Key]);
            points.Add(new PortfolioHistoryPoint(trade.CreatedAt, cash + holdingsValue));
        }

        points.Add(new PortfolioHistoryPoint(DateTime.UtcNow, current.TotalBalance));
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

        return await CreatePortfolioResponseAsync(portfolio, cancellationToken);
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

    private async Task<PortfolioResponse> CreatePortfolioResponseAsync(
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
            todaysGain += CalculateTodaysGain(item, quote, trades);
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

        var totalGains = holdingsValue - totalCost;

        return new PortfolioResponse(
            portfolio.Cash + holdingsValue,
            portfolio.Cash,
            todaysGain,
            Percent(todaysGain, holdingsValue - todaysGain),
            totalGains,
            Percent(totalGains, totalCost),
            holdingResponses);
    }

    private static decimal Percent(decimal part, decimal whole)
        => whole == 0 ? 0 : Math.Round(part / whole * 100, 2);

    private static decimal CashChange(Transaction trade)
    => trade.Type == "BUY"
        ? -trade.Price * trade.Quantity
        : trade.Price * trade.Quantity;

    private static decimal CalculateTodaysGain(Holding item, StockQuote? quote, List<Transaction> trades)
    {
        if (quote is null)
        {
            return 0;
        }

        var gain = (quote.Current - quote.PreviousClose) * item.Quantity;

        var tradesSinceClose = trades.Where(t =>
            t.Stock.Symbol == item.Stock.Symbol
            && t.CreatedAt.Date >= quote.UpdatedAt.Date);

        foreach (var trade in tradesSinceClose)
        {
            var moveBeforeTrade = (trade.Price - quote.PreviousClose) * trade.Quantity;
            gain += trade.Type == "BUY" ? -moveBeforeTrade : moveBeforeTrade;
        }

        return gain;
    }
}