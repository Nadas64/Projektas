using backend.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace backend.Data.Repositories.Implementations;

public class Repository<T>(AppDbContext db) : IRepository<T> where T : class
{
    protected AppDbContext Db { get; } = db;

    private readonly DbSet<T> _set = db.Set<T>();

    public Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _set.FindAsync([id], cancellationToken).AsTask();

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _set.AddAsync(entity, cancellationToken);

    public void Remove(T entity) => _set.Remove(entity);

    protected Task<List<T>> ListAsync<TKey>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, object>> include,
        Expression<Func<T, TKey>> orderBy,
    bool descending = false,
    CancellationToken cancellationToken = default)
    {
        var query = _set.AsNoTracking().Where(filter).Include(include);
        var ordered = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
        return ordered.ToListAsync(cancellationToken);
    }
}
