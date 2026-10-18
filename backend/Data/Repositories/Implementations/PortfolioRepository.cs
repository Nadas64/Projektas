using backend.Models;
using Microsoft.EntityFrameworkCore;
using backend.Data.Repositories.Interfaces;

namespace backend.Data.Repositories.Implementations;

public class PortfolioRepository(AppDbContext db) : Repository<Portfolio>(db), IPortfolioRepository
{
    public Task<Portfolio?> GetByUserIdAsync(int userId, CancellationToken ct = default)
        => Db.Portfolios.SingleOrDefaultAsync(p => p.UserId == userId, ct);

    public async Task<Portfolio?> GetByIdForUpdateAsync(int id, CancellationToken ct = default)
    {
        var tracked = Db.Portfolios.Local.FirstOrDefault(portfolio => portfolio.Id == id);
        if (tracked is not null)
        {
            Db.Entry(tracked).State = EntityState.Detached;
        }

        return await Db.Portfolios
            .FromSqlInterpolated($"SELECT * FROM \"Portfolios\" WHERE \"Id\" = {id} FOR UPDATE")
            .SingleOrDefaultAsync(ct);
    }
}
