using backend.Data.Repositories.Interfaces;
using backend.Dtos;
using backend.Exceptions;
using backend.Gateways.Interfaces;
using backend.Models;

namespace backend.Services;

public class WatchlistService(IUnitOfWork uow, IFinnhubGateway finnhub)
{
    public async Task<IReadOnlyList<WatchlistItemResponse>> GetWatchlistAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        var user = await GetUserAsync(username, cancellationToken);
        var items = await uow.Watchlist.GetByUserAsync(user.Id, cancellationToken);

        return items
            .Select(w => new WatchlistItemResponse(w.Stock.Symbol, w.Stock.CompanyName, w.CreatedAt))
            .ToList();
    }

    public async Task<IReadOnlyList<WatchlistItemResponse>> AddAsync(
        string username,
        string symbol,
        CancellationToken cancellationToken = default)
    {
        symbol = NormalizeSymbol(symbol);
        var user = await GetUserAsync(username, cancellationToken);

        var existing = await uow.Watchlist.GetByUserAndSymbolAsync(user.Id, symbol, cancellationToken);
        if (existing is not null)
        {
            throw new AppException(StatusCodes.Status409Conflict, "This stock is already in the watchlist.");
        }

        var stock = await uow.Stocks.GetBySymbolAsync(symbol, cancellationToken);
        if (stock is null)
        {
            var price = await finnhub.GetCurrentPriceAsync(symbol, cancellationToken);
            if (price is null)
            {
                throw new AppException(StatusCodes.Status404NotFound, $"Stock '{symbol}' was not found.");
            }

            var companyName = await finnhub.GetCompanyNameAsync(symbol, cancellationToken);
            stock = new Stock { Symbol = symbol, CompanyName = companyName ?? symbol };
            await uow.Stocks.AddAsync(stock, cancellationToken);
        }

        var item = new WatchlistItem
        {
            User = user,
            Stock = stock,
            CreatedAt = DateTime.UtcNow
        };
        await uow.Watchlist.AddAsync(item, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return await GetWatchlistAsync(username, cancellationToken);
    }

    public async Task<IReadOnlyList<WatchlistItemResponse>> RemoveAsync(
        string username,
        string symbol,
        CancellationToken cancellationToken = default)
    {
        symbol = NormalizeSymbol(symbol);
        var user = await GetUserAsync(username, cancellationToken);

        var item = await uow.Watchlist.GetByUserAndSymbolAsync(user.Id, symbol, cancellationToken)
                   ?? throw new AppException(StatusCodes.Status404NotFound, "This stock is not in the watchlist.");

        uow.Watchlist.Remove(item);
        await uow.SaveChangesAsync(cancellationToken);

        return await GetWatchlistAsync(username, cancellationToken);
    }

    private async Task<User> GetUserAsync(string username, CancellationToken cancellationToken)
        => await uow.Users.GetByUsernameAsync(username, cancellationToken)
           ?? throw new AppException(StatusCodes.Status404NotFound, $"User '{username}' was not found.");

    private static string NormalizeSymbol(string symbol)
    {
        var normalized = symbol.Trim().ToUpperInvariant();
        if (normalized.Length == 0)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "A stock symbol is required.");
        }

        return normalized;
    }
}
