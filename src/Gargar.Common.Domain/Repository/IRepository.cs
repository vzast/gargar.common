using Gargar.Common.Domain.Helpers;
using System.Linq.Expressions;

namespace Gargar.Common.Domain.Repository;

public interface IRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    Task<List<TEntity>> GetListForUpdateAsync(string[]? relatedProperties = null, Expression<Func<TEntity, bool>>? predicate = null, int? skip = null, int? take = null, SortingDetails? sortingDetails = null, CancellationToken cancellationToken = default);

    Task<TEntity?> GetForUpdateAsync(object key, string[]? relatedProperties = null, CancellationToken cancellationToken = default);

    Task<TEntity?> GetForUpdateAsync(Expression<Func<TEntity, bool>> predicate, string[]? relatedProperties = null, CancellationToken cancellationToken = default);

    Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<TEntity?> UpdateAsync(object key, Func<TEntity, Task> updateAction, CancellationToken cancellationToken = default);

    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(object key, CancellationToken cancellationToken = default);

    Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
}