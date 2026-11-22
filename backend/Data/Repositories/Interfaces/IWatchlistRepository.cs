using backend.Models;

namespace backend.Data.Repositories.Interfaces;

public interface IWatchlistRepository : IRepository<WatchlistItem>
{
    Task<List<WatchlistItem>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<WatchlistItem?> GetByUserAndSymbolAsync(
        int userId,
        string symbol,
        CancellationToken cancellationToken = default);
}
