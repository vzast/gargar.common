using Gargar.Common.Application.Interfaces;
using Gargar.Common.Domain.Repository;
using Gargar.Common.Persistance.Database;
using Gargar.Common.Persistance.Repository;
using Microsoft.EntityFrameworkCore.Storage;

namespace Gargar.Common.Persistance.UoW;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly AppDbContext _context = context;
    private readonly Dictionary<string, object> _repositories = new();
    private IDbContextTransaction? _transaction;

    public IBaseRepository<TEntity, TKey> Repository<TEntity, TKey>() where TEntity : class, new()
    {
        var type = typeof(TEntity).Name;
        if (_repositories.TryGetValue(type, out var repo))
            return (IBaseRepository<TEntity, TKey>)repo;

        var repositoryInstance = new BaseRepository<TEntity, TKey>(_context);
        _repositories.Add(type, repositoryInstance);
        return repositoryInstance;
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
            return;

        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            await _transaction?.CommitAsync()!;
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await DisposeTransactionAsync();
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        await DisposeTransactionAsync();
        await _context.DisposeAsync();
    }
}