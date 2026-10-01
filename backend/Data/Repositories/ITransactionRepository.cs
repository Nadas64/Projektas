using backend.Models;

namespace backend.Data.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<List<Transaction>> GetByPortfolioAsync(int portfolioId, CancellationToken ct = default);
}
