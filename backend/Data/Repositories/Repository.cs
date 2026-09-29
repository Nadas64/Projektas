using Microsoft.EntityFrameworkCore;

namespace backend.Data.Repositories;

public class EfRepository<T>(AppDbContext db) : IRepository<T> where T : class
{
    protected AppDbContext Db { get; } = db;

    private readonly DbSet<T> _set = db.Set<T>();

    public Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => _set.FindAsync([id], ct).AsTask();

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _set.AddAsync(entity, ct);

    public void Remove(T entity) => _set.Remove(entity);
}
