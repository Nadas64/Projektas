using backend.Models;

namespace backend.Data.Repositories.Interfaces;

public interface IStockRepository : IRepository<Stock>
{
    Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default);

    Task<List<Stock>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
