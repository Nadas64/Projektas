using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Repositories;

public class UserRepository(AppDbContext db) : EfRepository<User>(db), IUserRepository
{
    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => Db.Users.SingleOrDefaultAsync(u => u.Username == username, ct);

    public Task<User?> GetByUsernameWithPortfolioAsync(string username, CancellationToken ct = default)
        => Db.Users.Include(u => u.Portfolio).SingleOrDefaultAsync(u => u.Username == username, ct);

    public Task<User?> GetByIdWithPortfolioAsync(int id, CancellationToken ct = default)
        => Db.Users.Include(u => u.Portfolio).SingleOrDefaultAsync(u => u.Id == id, ct);

    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
        => Db.Users.AnyAsync(u => u.Username == username, ct);
}
