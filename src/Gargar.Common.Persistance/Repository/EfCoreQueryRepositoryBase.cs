using Gargar.Common.Domain.Extentions;
using Gargar.Common.Domain.Helpers;
using Gargar.Common.Domain.Options;
using Gargar.Common.Domain.Repository;
using Gargar.Common.Persistance.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Linq.Expressions;

namespace Gargar.Common.Persistance.Repository;

public class EfCoreQueryRepositoryBase<TDbContext, TEntity>(TDbContext context, RepositoryOptions<TDbContext> repositoryOptions) : IQueryRepository<TEntity> where TDbContext : DbContext where TEntity : class
{
    protected readonly TDbContext Context = context;

    protected readonly RepositoryOptions<TDbContext> RepositoryOptions = repositoryOptions;

    private static SortingDetails? s_defaultSorting;

    private static readonly Lock s_defaultSortingLock = new();

    private static List<string>? s_primaryKeys;

    private static readonly Lock s_primaryKeysLock = new();

    protected DbSet<TEntity> Table => Context.Set<TEntity>();

    public virtual Task<List<TEntity>> GetListAsync(string[]? relatedProperties = null, Expression<Func<TEntity, bool>>? predicate = null, int? skip = null, int? take = null, SortingDetails? sortingDetails = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> source = EntityFrameworkQueryableExtensions.AsNoTracking(Table.ApplyIncludes(relatedProperties, RepositoryOptions.RelatedPropertiesMaxDepth, forQuerying: true).AsQueryable());
        return GetListAsync(source, predicate, skip, take, sortingDetails, cancellationToken);
    }

    public Task<List<TResult>> GetListAsync<TResult>(Expression<Func<TEntity, TResult>> projection, string[]? relatedProperties = null, Expression<Func<TEntity, bool>>? predicate = null, int? skip = null, int? take = null, SortingDetails? sortingDetails = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> source = EntityFrameworkQueryableExtensions.AsNoTracking(Table.ApplyIncludes(relatedProperties, RepositoryOptions.RelatedPropertiesMaxDepth, forQuerying: true).AsQueryable());
        return GetListAsync(source, projection, predicate, skip, take, sortingDetails, cancellationToken);
    }

    public virtual async Task<PagedList<TEntity>> GetPagedListAsync(int pageIndex, int pageSize, string[]? relatedProperties = null, Expression<Func<TEntity, bool>>? predicate = null, SortingDetails? sortingDetails = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = EntityFrameworkQueryableExtensions.AsNoTracking(Table.ApplyIncludes(relatedProperties, RepositoryOptions.RelatedPropertiesMaxDepth, forQuerying: true));
        if (predicate != null)
        {
            queryable = queryable.Where(predicate);
        }

        IQueryable<TEntity> source = queryable;
        SortingDetails sortingDetails2;
        if (sortingDetails != null)
        {
            List<SortItem> sortItems = sortingDetails.SortItems;
            if (sortItems != null && sortItems.Count > 0)
            {
                sortingDetails2 = sortingDetails;
                goto IL_007e;
            }
        }

        sortingDetails2 = GetDefaultSorting();
        goto IL_007e;
    IL_007e:
        queryable = source.OrderBy(sortingDetails2);
        return await PagedListHelper.Create(queryable, pageIndex, pageSize, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
    }

    public async Task<PagedList<TResult>> GetPagedListAsync<TResult>(Expression<Func<TEntity, TResult>> projection, int pageIndex, int pageSize, string[]? relatedProperties = null, Expression<Func<TEntity, bool>>? predicate = null, SortingDetails? sortingDetails = null, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(projection, "projection");
        IQueryable<TEntity> queryable = EntityFrameworkQueryableExtensions.AsNoTracking(Table.ApplyIncludes(relatedProperties, RepositoryOptions.RelatedPropertiesMaxDepth, forQuerying: true));
        if (predicate != null)
        {
            queryable = queryable.Where(predicate);
        }

        IQueryable<TEntity> source = queryable;
        SortingDetails sortingDetails2;
        if (sortingDetails != null)
        {
            List<SortItem> sortItems = sortingDetails.SortItems;
            if (sortItems != null && sortItems.Count > 0)
            {
                sortingDetails2 = sortingDetails;
                goto IL_008f;
            }
        }

        sortingDetails2 = GetDefaultSorting();
        goto IL_008f;
    IL_008f:
        queryable = source.OrderBy(sortingDetails2);
        return await PagedListHelper.Create(queryable.Select(projection), pageIndex, pageSize, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
    }

    protected Task<List<TEntity>> GetListAsync(IQueryable<TEntity> source, Expression<Func<TEntity, bool>>? predicate = null, int? skip = null, int? take = null, SortingDetails? sortingDetails = null, CancellationToken cancellationToken = default)
    {
        return EntityFrameworkQueryableExtensions.ToListAsync(ApplyQueryParameters(source, predicate, skip, take, sortingDetails), cancellationToken);
    }

    protected Task<List<TResult>> GetListAsync<TResult>(IQueryable<TEntity> source, Expression<Func<TEntity, TResult>> projection, Expression<Func<TEntity, bool>>? predicate = null, int? skip = null, int? take = null, SortingDetails? sortingDetails = null, CancellationToken cancellationToken = default)
    {
        return EntityFrameworkQueryableExtensions.ToListAsync(ApplyQueryParameters(source, predicate, skip, take, sortingDetails).Select(projection), cancellationToken);
    }

    private IQueryable<TEntity> ApplyQueryParameters(IQueryable<TEntity> source, Expression<Func<TEntity, bool>>? predicate = null, int? skip = null, int? take = null, SortingDetails? sortingDetails = null)
    {
        IQueryable<TEntity> queryable = source;
        if (predicate != null)
        {
            queryable = queryable.Where(predicate);
        }

        IQueryable<TEntity> source2 = queryable;
        SortingDetails sortingDetails2;
        if (sortingDetails != null)
        {
            List<SortItem> sortItems = sortingDetails.SortItems;
            if (sortItems != null && sortItems.Count > 0)
            {
                sortingDetails2 = sortingDetails;
                goto IL_0030;
            }
        }

        sortingDetails2 = GetDefaultSorting();
        goto IL_0030;
    IL_0030:
        queryable = source2.OrderBy(sortingDetails2);
        if (skip.HasValue && skip.GetValueOrDefault() > 0)
        {
            queryable = queryable.Skip(skip.Value);
        }

        if (take.HasValue && take.GetValueOrDefault() > 0)
        {
            queryable = queryable.Take(take.Value);
        }

        return queryable;
    }

    public virtual async Task<TEntity?> GetAsync(object key, string[]? relatedProperties = null, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(key, "key");
        return await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(EntityFrameworkQueryableExtensions.AsNoTracking(Table.ApplyIncludes(relatedProperties, RepositoryOptions.RelatedPropertiesMaxDepth, forQuerying: true).AsQueryable()), GetByKeyExpression(key), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
    }

    public async Task<TResult?> GetAsync<TResult>(Expression<Func<TEntity, TResult>> projection, object key, string[]? relatedProperties = null, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(projection, "projection");
        Guard.NotNull(key, "key");
        return await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(EntityFrameworkQueryableExtensions.AsNoTracking(Table.ApplyIncludes(relatedProperties, RepositoryOptions.RelatedPropertiesMaxDepth, forQuerying: true).AsQueryable()).Where(GetByKeyExpression(key)).Select(projection), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
    }

    public virtual async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, string[]? relatedProperties = null, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(predicate, "predicate");
        return await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(EntityFrameworkQueryableExtensions.AsNoTracking(Table.ApplyIncludes(relatedProperties, RepositoryOptions.RelatedPropertiesMaxDepth, forQuerying: true).AsQueryable()), predicate, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
    }

    public async Task<TResult?> GetAsync<TResult>(Expression<Func<TEntity, TResult>> projection, Expression<Func<TEntity, bool>> predicate, string[]? relatedProperties = null, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(projection, "projection");
        Guard.NotNull(predicate, "predicate");
        return await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(EntityFrameworkQueryableExtensions.AsNoTracking(Table.ApplyIncludes(relatedProperties, RepositoryOptions.RelatedPropertiesMaxDepth, forQuerying: true).AsQueryable()).Where(predicate).Select(projection), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
    }

    public virtual Task<long> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> source = EntityFrameworkQueryableExtensions.AsNoTracking(Table.AsQueryable());
        if (predicate != null)
        {
            source = source.Where(predicate);
        }

        return EntityFrameworkQueryableExtensions.LongCountAsync(source, cancellationToken);
    }

    public virtual Task<bool> ExistsAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> source = EntityFrameworkQueryableExtensions.AsNoTracking(Table.AsQueryable());
        if (predicate != null)
        {
            source = source.Where(predicate);
        }

        return EntityFrameworkQueryableExtensions.AnyAsync(source, cancellationToken);
    }

    protected Expression<Func<TEntity, bool>> GetByKeyExpression(object keyObjectArray)
    {
        object[] array = ((keyObjectArray is IEnumerable) ? ((object[])keyObjectArray) : [keyObjectArray]);
        Type typeFromHandle = typeof(TEntity);
        List<string> primaryKeyFields = GetPrimaryKeyFields(typeFromHandle);
        if (primaryKeyFields == null || primaryKeyFields.Count <= 0)
        {
            throw new RepositoryException("Primary key for entity " + typeFromHandle.FullName + " not found");
        }

        if (primaryKeyFields.Count != array.Length)
        {
            throw new RepositoryException("Primary key fields and provided arguments quantity does not match for entity " + typeFromHandle.FullName);
        }

        ParameterExpression parameterExpression = Expression.Parameter(typeFromHandle, "x");
        Expression expression = null;
        for (int i = 0; i < primaryKeyFields.Count; i++)
        {
            string propertyName = primaryKeyFields[i];
            MemberExpression left = Expression.Property(parameterExpression, propertyName);
            ConstantExpression right = Expression.Constant(array[i]);
            BinaryExpression binaryExpression = Expression.Equal(left, right);
            expression = (expression == null) ? binaryExpression : Expression.AndAlso(expression, binaryExpression);
        }

        return Expression.Lambda<Func<TEntity, bool>>(expression, [parameterExpression]);
    }

    protected SortingDetails GetDefaultSorting()
    {
        if (s_defaultSorting != null)
        {
            return s_defaultSorting;
        }

        lock (s_defaultSortingLock)
        {
            List<string> primaryKeyFields = GetPrimaryKeyFields(typeof(TEntity));
            if (primaryKeyFields == null || primaryKeyFields.Count <= 0)
            {
                s_defaultSorting = new SortingDetails();
            }
            else
            {
                s_defaultSorting = new SortingDetails([.. primaryKeyFields.Select(x => new SortItem(x, SortDirection.Ascending))]);
            }
        }

        return s_defaultSorting;
    }

    protected List<string> GetPrimaryKeyFields(Type entityType)
    {
        if (s_primaryKeys != null)
        {
            return s_primaryKeys;
        }

        lock (s_primaryKeysLock)
        {
            s_primaryKeys = Context.Model.FindEntityType(entityType)?.GetKeys().FirstOrDefault(x => x.IsPrimaryKey())?.Properties.Select(y => y.Name).ToList() ?? [];
        }

        return s_primaryKeys;
    }
}