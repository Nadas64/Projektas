namespace backend.Data.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Remove(T entity);
}
