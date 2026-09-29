using backend.Models;

namespace backend.Data.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);

    Task<User?> GetByUsernameWithPortfolioAsync(string username, CancellationToken ct = default);

    Task<User?> GetByIdWithPortfolioAsync(int id, CancellationToken ct = default);

    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);
}
