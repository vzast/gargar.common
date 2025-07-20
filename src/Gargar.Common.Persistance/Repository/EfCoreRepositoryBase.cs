using Gargar.Common.Domain.Extentions;
using Gargar.Common.Domain.Helpers;
using Gargar.Common.Domain.Options;
using Gargar.Common.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Gargar.Common.Persistance.Repository;

public class EfCoreRepositoryBase<TDbContext, TEntity>(TDbContext context, RepositoryOptions<TDbContext> repositoryOptions) : EfCoreQueryRepositoryBase<TDbContext, TEntity>(context, repositoryOptions), IRepository<TEntity>, IQueryRepository<TEntity> where TDbContext : DbContext where TEntity : class
{
    public virtual Task<List<TEntity>> GetListForUpdateAsync(string[]? relatedProperties = null, Expression<Func<TEntity, bool>>? predicate = null, int? skip = null, int? take = null, SortingDetails? sortingDetails = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> source = EntityFrameworkQueryableExtensions.AsTracking(base.Table.ApplyIncludes(relatedProperties, RepositoryOptions.RelatedPropertiesMaxDepth).AsQueryable());
        return GetListAsync(source, predicate, skip, take, sortingDetails, cancellationToken);
    }

    public virtual async Task<TEntity?> GetForUpdateAsync(object key, string[]? relatedProperties = null, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(key, "key");
        return await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(EntityFrameworkQueryableExtensions.AsTracking(base.Table.ApplyIncludes(relatedProperties, RepositoryOptions.RelatedPropertiesMaxDepth).AsQueryable()), GetByKeyExpression(key), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
    }

    public virtual async Task<TEntity?> GetForUpdateAsync(Expression<Func<TEntity, bool>> predicate, string[]? relatedProperties = null, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(predicate, "predicate");
        return await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(EntityFrameworkQueryableExtensions.AsTracking(base.Table.ApplyIncludes(relatedProperties, RepositoryOptions.RelatedPropertiesMaxDepth).AsQueryable()), predicate, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
    }

    public virtual async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(entity, "entity");
        await base.Table.AddAsync(entity, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        if (!Context.Entry(entity).IsKeySet)
        {
            await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

        await SaveChanges(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        return entity;
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(entity, "entity");
        await SaveChanges(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        return entity;
    }

    public virtual async Task<TEntity?> UpdateAsync(object key, Func<TEntity, Task> updateAction, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(key, "key");
        Guard.NotNull(updateAction, "updateAction");
        TEntity entity = await GetForUpdateAsync(key, null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        if (entity != null)
        {
            await updateAction(entity).ConfigureAwait(continueOnCapturedContext: false);
            await SaveChanges(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

        return entity;
    }

    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(entity, "entity");
        base.Table.Remove(entity);
        await SaveChanges(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
    }

    public virtual async Task DeleteAsync(object key, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(key, "key");
        TEntity? val = await GetForUpdateAsync(key, null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        if (val != null)
        {
            base.Table.Remove(val);
            await SaveChanges(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }
    }

    public virtual async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        List<TEntity> list = await GetListForUpdateAsync(null, predicate, null, null, null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        if (list.Count > 0)
        {
            foreach (TEntity item in list)
            {
                base.Table.Remove(item);
            }
        }

        await SaveChanges(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
    }

    private Task SaveChanges(CancellationToken cancellationToken)
    {
        if (RepositoryOptions.SaveChangesStrategy == SaveChangesStrategy.PerOperation)
        {
            return Context.SaveChangesAsync(cancellationToken);
        }

        if (RepositoryOptions.SaveChangesStrategy == SaveChangesStrategy.PerUnitOfWork && Context.Database.CurrentTransaction == null)
        {
            return Context.SaveChangesAsync(cancellationToken);
        }

        return Task.CompletedTask;
    }
}