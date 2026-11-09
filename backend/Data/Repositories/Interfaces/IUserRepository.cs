using backend.Models;

namespace backend.Data.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<User?> GetByUsernameWithPortfolioAsync(string username, CancellationToken cancellationToken = default);

    Task<User?> GetByIdWithPortfolioAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);

    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
}
