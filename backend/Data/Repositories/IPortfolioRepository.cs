using backend.Models;

namespace backend.Data.Repositories;

public interface IPortfolioRepository : IRepository<Portfolio>
{
    Task<Portfolio?> GetByUserIdAsync(int userId, CancellationToken ct = default);

    Task<Portfolio?> GetByIdForUpdateAsync(int id, CancellationToken ct = default);
}
