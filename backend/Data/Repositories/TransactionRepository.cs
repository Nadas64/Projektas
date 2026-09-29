using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Repositories;

public class TransactionRepository(AppDbContext db) : EfRepository<Transaction>(db), ITransactionRepository
{
    public Task<List<Transaction>> GetByPortfolioAsync(int portfolioId, CancellationToken ct = default)
        => Db.Transactions
            .Where(t => t.PortfolioId == portfolioId)
            .Include(t => t.Stock)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
}
