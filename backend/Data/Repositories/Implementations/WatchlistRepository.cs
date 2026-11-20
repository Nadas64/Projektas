using backend.Data.Repositories.Interfaces;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Repositories.Implementations;

public class WatchlistRepository(AppDbContext db) : Repository<WatchlistItem>(db), IWatchlistRepository
{
    public Task<List<WatchlistItem>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
        => ListAsync(w => w.UserId == userId, w => w.Stock, w => w.Stock.Symbol, cancellationToken: cancellationToken);

    public Task<WatchlistItem?> GetByUserAndSymbolAsync(
        int userId,
        string symbol,
        CancellationToken cancellationToken = default)
        => Db.Watchlists
            .Include(w => w.Stock)
            .SingleOrDefaultAsync(w => w.UserId == userId && w.Stock.Symbol == symbol, cancellationToken);
}
