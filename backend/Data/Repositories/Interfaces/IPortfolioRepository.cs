using backend.Models;

namespace backend.Data.Repositories.Interfaces;

public interface IPortfolioRepository : IRepository<Portfolio>
{
    Task<Portfolio?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
