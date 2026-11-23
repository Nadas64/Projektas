using backend.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace backend.Data.Repositories.Implementations;

public class UnitOfWork(
    AppDbContext db,
    IUserRepository users,
    IPortfolioRepository portfolios,
    IStockRepository stocks,
    IHoldingRepository holdings,
    ITransactionRepository transactions,
    IWatchlistRepository watchlist)
    : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public IUserRepository Users { get; } = users;
    public IPortfolioRepository Portfolios { get; } = portfolios;
    public IStockRepository Stocks { get; } = stocks;
    public IHoldingRepository Holdings { get; } = holdings;
    public ITransactionRepository Transactions { get; } = transactions;
    public IWatchlistRepository Watchlist { get; } = watchlist;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            throw new InvalidOperationException("A transaction is already in progress.");

        _transaction = await db.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("There is no transaction to commit.");

        var transaction = _transaction;

        try
        {
            await transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            _transaction = null;
            await transaction.DisposeAsync();
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            return;

        var transaction = _transaction;
        _transaction = null;

        try
        {
            await transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        if (_transaction is null)
            return;

        var transaction = _transaction;
        _transaction = null;

        try
        {
            await transaction.RollbackAsync();
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }
}