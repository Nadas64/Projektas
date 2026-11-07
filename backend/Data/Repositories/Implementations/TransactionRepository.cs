using backend.Data.Repositories.Interfaces;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Repositories.Implementations;

public class TransactionRepository(AppDbContext db) : Repository<Transaction>(db), ITransactionRepository
{
    public Task<List<Transaction>> GetByPortfolioAsync(int portfolioId, CancellationToken cancellationToken = default)
        => ListAsync(t => t.PortfolioId == portfolioId, t => t.Stock, t => t.CreatedAt, descending: true, cancellationToken: cancellationToken);
}
