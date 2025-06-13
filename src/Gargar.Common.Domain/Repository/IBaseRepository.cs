using Distribution.Domain.Helpers;
using Gargar.Common.Domain.Helpers;
using System.Linq.Expressions;

namespace Distribution.Domain.Repository;

public interface IBaseRepository<T, TKey> where T : class
{
    ValueTask<T?> GetByIdAsync(TKey id);
    ValueTask<T?> GetByIdAsync(TKey id, params string[] includeProperties);

    ValueTask<T?> Get(params object?[]? keyValues);
    ValueTask<T?> Get(object?[]? keyValues, params string[] includeProperties);

    Task<T?> Get(Expression<Func<T, bool>> predicate);
    Task<T?> Get(Expression<Func<T, bool>> predicate, params string[] includeProperties);

    Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate);
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate, params string[] includeProperties);

    Task<PagedList<T>> GetPagedAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
        int pageNumber = 1,
        int pageSize = 32);

    Task<PagedList<T>> GetPagedAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
        int pageNumber = 1,
        int pageSize = 32,
        params string[] includeProperties);

    // These methods remain unchanged
    ValueTask AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> req);
    void Update(T entity);
    Task<T> Update(object id, object updateData);
    void DeleteAsync(T entity);
    Task DeleteAsync(TKey key);
    Task ExecuteDeleteAsync(Expression<Func<T, bool>> predecate);
    Task<bool> ExistsAsync();
    Task<bool> ExistsAsync(TKey id);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task SaveAsync();
}