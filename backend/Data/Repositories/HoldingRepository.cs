using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Repositories;

public class HoldingRepository(AppDbContext db) : EfRepository<Holding>(db), IHoldingRepository
{
    public Task<List<Holding>> GetByPortfolioAsync(int portfolioId, CancellationToken ct = default)
        => Db.Holdings
            .Where(h => h.PortfolioId == portfolioId)
            .Include(h => h.Stock)
            .OrderBy(h => h.Stock.Symbol)
            .ToListAsync(ct);

    public Task<Holding?> GetByPortfolioAndSymbolAsync(
        int portfolioId,
        string symbol,
        CancellationToken ct = default)
        => Db.Holdings
            .Include(h => h.Stock)
            .SingleOrDefaultAsync(h => h.PortfolioId == portfolioId && h.Stock.Symbol == symbol, ct);
}
