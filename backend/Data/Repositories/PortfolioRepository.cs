using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Repositories;

public class PortfolioRepository(AppDbContext db) : EfRepository<Portfolio>(db), IPortfolioRepository
{
    public Task<Portfolio?> GetByUserIdAsync(int userId, CancellationToken ct = default)
        => Db.Portfolios.SingleOrDefaultAsync(p => p.UserId == userId, ct);

    public Task<Portfolio?> GetByIdForUpdateAsync(int id, CancellationToken ct = default)
        => Db.Portfolios
            .FromSqlInterpolated($"SELECT * FROM \"Portfolios\" WHERE \"Id\" = {id} FOR UPDATE")
            .SingleOrDefaultAsync(ct);
}
