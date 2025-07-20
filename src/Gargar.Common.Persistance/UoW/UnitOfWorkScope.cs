using Gargar.Common.Application.Interfaces;
using Gargar.Common.Domain.Repository;
using Gargar.Common.Persistance.Database;
using Gargar.Common.Persistance.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Gargar.Common.Persistance.UoW;

internal sealed class UnitOfWorkScope(IServiceProvider serviceProvider) : IUnitOfWorkScope
{
    private int _index;

    private readonly IServiceProvider _scopeServiceProvider = serviceProvider;

    private readonly List<DbContext> _contexts = [];
    private readonly List<(IDbContextTransaction transaction, DbContext context)> _transactions = [];

    public bool IsCompleted { get; private set; }
    public bool IsRolledBack { get; private set; }

    public async Task BeginAsync(CancellationToken cancellationToken)
    {
        if (IsCompleted)
            throw new UnitOfWorkException("Unit of work scope is already completed");

        if (_index == 0)
            await BeginTransactionsAsync(cancellationToken).ConfigureAwait(false);

        _index++;
    }

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class => _scopeServiceProvider.GetService<IRepository<TEntity>>() ?? throw new UnitOfWorkException($"Repository of type {typeof(TEntity)} not found");

    public TService GetService<TService>() where TService : class => _scopeServiceProvider.GetService<TService>() ?? throw new UnitOfWorkException($"Service of type {typeof(TService)} not found");

    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        if (IsRolledBack)
            throw new UnitOfWorkException("Unit of work scope is rolled back");

        if (IsCompleted)
            return;

        if (_index == 1)
        {
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            CommitTransactions();
            IsCompleted = true;
        }
    }

    public void Dispose()
    {
        if (_index > 0)
            _index--;

        if (!IsCompleted && _index == 0)
        {
            DetachAllEntities();
            RollbackTransactions();
            IsRolledBack = true;
            IsCompleted = true;
        }
    }

    private async Task BeginTransactionsAsync(CancellationToken cancellationToken)
    {
        ResolveContexts();
        foreach (var context in _contexts.Where(static context => HasTransactionManager(context)))
        {
            var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            _transactions.Add((transaction, context));
        }
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        foreach (var context in _contexts)
        {
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private void DetachAllEntities()
    {
        foreach (var context in _contexts)
        {
            var changedEntriesCopy = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in changedEntriesCopy)
                entry.State = EntityState.Detached;
        }
    }

    private void CommitTransactions()
    {
        var transactions = _transactions.ToList();
        foreach (var transaction in transactions)
        {
            transaction.transaction.Commit();
            transaction.transaction.Dispose();
            _transactions.Remove(transaction);
        }
    }

    private void RollbackTransactions()
    {
        var transactions = _transactions.ToList();
        foreach (var transaction in transactions)
        {
            transaction.transaction.Rollback();
            transaction.transaction.Dispose();
            _transactions.Remove(transaction);
        }
    }

    private void ResolveContexts()
    {
        foreach (var contextType in DbContexts.GetContextTypes)
        {
            if (_scopeServiceProvider?.GetService(contextType) is DbContext context)
                _contexts.Add(context);
        }
    }

    private static bool HasTransactionManager(DbContext dbContext)
    {
        return ((IDatabaseFacadeDependenciesAccessor)dbContext.Database).Dependencies.TransactionManager is
            IRelationalTransactionManager;
    }
}