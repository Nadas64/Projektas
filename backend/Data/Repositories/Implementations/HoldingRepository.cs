using backend.Data.Repositories.Interfaces;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Repositories.Implementations;

public class HoldingRepository(AppDbContext db) : Repository<Holding>(db), IHoldingRepository
{
    public Task<List<Holding>> GetByPortfolioAsync(int portfolioId, CancellationToken cancellationToken = default)
        => ListAsync(h => h.PortfolioId == portfolioId, h => h.Stock, h => h.Stock.Symbol, cancellationToken: cancellationToken);

    public Task<Holding?> GetByPortfolioAndSymbolAsync(
        int portfolioId,
        string symbol,
        CancellationToken cancellationToken = default)
        => Db.Holdings
            .Include(h => h.Stock)
            .SingleOrDefaultAsync(h => h.PortfolioId == portfolioId && h.Stock.Symbol == symbol, cancellationToken);
}
