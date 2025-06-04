using Gargar.Common.Domain.Repository;

namespace Gargar.Common.Application.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IBaseRepository<TEntity, TKey> Repository<TEntity, TKey>() where TEntity : class, new();

    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync();

    Task CommitTransactionAsync();

    Task RollbackTransactionAsync();
}