using backend.Models;

namespace backend.Data.Repositories.Interfaces;

public interface IPortfolioRepository : IRepository<Portfolio>
{
    Task<Portfolio?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<Portfolio?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);

    Task<List<Portfolio>> GetAllAsync(CancellationToken cancellationToken = default);
}
