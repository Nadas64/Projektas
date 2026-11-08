using backend.Data.Repositories.Interfaces;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Repositories.Implementations;

public class PortfolioRepository(AppDbContext db) : Repository<Portfolio>(db), IPortfolioRepository
{
    public Task<Portfolio?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        => Db.Portfolios.SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);
}
