using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Repositories;

public class StockRepository(AppDbContext db) : Repository<Stock>(db), IStockRepository
{
    public Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken ct = default)
        => Db.Stocks.SingleOrDefaultAsync(s => s.Symbol == symbol, ct);

    public Task<List<Stock>> SearchAsync(string query, CancellationToken ct = default)
        => Db.Stocks
            .AsNoTracking()
            .Where(s => EF.Functions.ILike(s.Symbol, $"%{query}%")
                || EF.Functions.ILike(s.CompanyName, $"%{query}%"))
            .OrderBy(s => s.Symbol)
            .ToListAsync(ct);
}
