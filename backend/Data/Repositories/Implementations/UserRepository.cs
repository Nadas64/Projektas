using backend.Data.Repositories.Interfaces;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Repositories.Implementations;

public class UserRepository(AppDbContext db) : Repository<User>(db), IUserRepository
{
    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => Db.Users.SingleOrDefaultAsync(u => u.Username == username, cancellationToken);

    public Task<User?> GetByUsernameWithPortfolioAsync(string username, CancellationToken cancellationToken = default)
        => Db.Users.Include(u => u.Portfolio).SingleOrDefaultAsync(u => u.Username == username, cancellationToken);

    public Task<User?> GetByIdWithPortfolioAsync(int id, CancellationToken cancellationToken = default)
        => Db.Users.Include(u => u.Portfolio).SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
        => Db.Users.AnyAsync(u => u.Username == username, cancellationToken);
}
