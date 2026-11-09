namespace backend.Data.Repositories.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IUserRepository Users { get; }

    IPortfolioRepository Portfolios { get; }

    IStockRepository Stocks { get; }

    IHoldingRepository Holdings { get; }

    ITransactionRepository Transactions { get; }

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
