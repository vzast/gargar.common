using Gargar.Common.Domain.Repository;

namespace Gargar.Common.Application.Interfaces;

public interface IUnitOfWork
{
    Task<IUnitOfWorkScope> CreateScopeAsync(CancellationToken cancellationToken = default);

    Task ExecuteAsync(Func<IUnitOfWorkScope, CancellationToken, Task> action, CancellationToken cancellationToken = default);
}

public interface IUnitOfWorkScope : IDisposable
{
    [Obsolete(message: "Use dependency injection to resolve repositories")]
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

    [Obsolete(message: "Use dependency injection to resolve services")]
    TService GetService<TService>() where TService : class;

    Task CompleteAsync(CancellationToken cancellationToken = default);
}