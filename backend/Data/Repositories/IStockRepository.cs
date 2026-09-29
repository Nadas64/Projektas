using backend.Models;

namespace backend.Data.Repositories;

public interface IStockRepository : IRepository<Stock>
{
    Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken ct = default);

    Task<List<Stock>> SearchAsync(string query, CancellationToken ct = default);
}
