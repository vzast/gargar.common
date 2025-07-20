using Gargar.Common.Domain.Helpers;
using System.Linq.Expressions;

namespace Gargar.Common.Domain.Repository;

public interface IQueryRepository<TEntity> where TEntity : class
{
    Task<List<TEntity>> GetListAsync(string[]? relatedProperties = null, Expression<Func<TEntity, bool>>? predicate = null, int? skip = null, int? take = null, SortingDetails? sortingDetails = null, CancellationToken cancellationToken = default);

    Task<List<TResult>> GetListAsync<TResult>(Expression<Func<TEntity, TResult>> projection, string[]? relatedProperties = null, Expression<Func<TEntity, bool>>? predicate = null, int? skip = null, int? take = null, SortingDetails? sortingDetails = null, CancellationToken cancellationToken = default);

    Task<PagedList<TEntity>> GetPagedListAsync(int pageIndex, int pageSize, string[]? relatedProperties = null, Expression<Func<TEntity, bool>>? predicate = null, SortingDetails? sortingDetails = null, CancellationToken cancellationToken = default);

    Task<PagedList<TResult>> GetPagedListAsync<TResult>(Expression<Func<TEntity, TResult>> projection, int pageIndex, int pageSize, string[]? relatedProperties = null, Expression<Func<TEntity, bool>>? predicate = null, SortingDetails? sortingDetails = null, CancellationToken cancellationToken = default);

    Task<TEntity?> GetAsync(object key, string[]? relatedProperties = null, CancellationToken cancellationToken = default);

    Task<TResult?> GetAsync<TResult>(Expression<Func<TEntity, TResult>> projection, object key, string[]? relatedProperties = null, CancellationToken cancellationToken = default);

    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, string[]? relatedProperties = null, CancellationToken cancellationToken = default);

    Task<TResult?> GetAsync<TResult>(Expression<Func<TEntity, TResult>> projection, Expression<Func<TEntity, bool>> predicate, string[]? relatedProperties = null, CancellationToken cancellationToken = default);

    Task<long> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
}