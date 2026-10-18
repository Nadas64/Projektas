using backend.Models;

namespace backend.Data.Repositories.Interfaces;

public interface IHoldingRepository : IRepository<Holding>
{
    Task<List<Holding>> GetByPortfolioAsync(int portfolioId, CancellationToken ct = default);

    Task<Holding?> GetByPortfolioAndSymbolAsync(
        int portfolioId,
        string symbol,
        CancellationToken ct = default);
}
