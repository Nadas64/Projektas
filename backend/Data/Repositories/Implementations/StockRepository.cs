using backend.Data.Repositories.Interfaces;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Repositories.Implementations;

public class StockRepository(AppDbContext db) : Repository<Stock>(db), IStockRepository
{
    public Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default)
        => Db.Stocks.SingleOrDefaultAsync(s => s.Symbol == symbol, cancellationToken);

    public Task<List<Stock>> SearchAsync(string query, CancellationToken cancellationToken = default)
        => Db.Stocks
            .AsNoTracking()
            .Where(s => EF.Functions.ILike(s.Symbol, $"%{query}%")
                || EF.Functions.ILike(s.CompanyName, $"%{query}%"))
            .OrderBy(s => s.Symbol)
            .ToListAsync(cancellationToken);
}
