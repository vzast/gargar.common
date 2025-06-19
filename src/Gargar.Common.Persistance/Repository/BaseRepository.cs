using Gargar.Common.Domain.Helpers;
using Gargar.Common.Domain.Repository;
using Gargar.Common.Persistance.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Gargar.Common.Persistance.Repository;

/// <summary>
/// Base repository implementation for data access operations
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
/// <typeparam name="TKey">The type of the primary key</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="BaseRepository{T, TKey}"/> class
/// </remarks>
/// <param name="context">The database context</param>
public class BaseRepository<T, TKey>(AppDbContext context) : IBaseRepository<T, TKey> where T : class, new()
{
    /// <summary>
    /// The database context
    /// </summary>
    protected readonly DbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Gets the DbSet for the entity
    /// </summary>
    public DbSet<T> Table { get; } = context.Set<T>();

    /// <summary>
    /// Applies string-based include properties to a query
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="includeProperties">The include property names</param>
    /// <returns>The query with includes applied</returns>
    private static IQueryable<T> ApplyIncludes(IQueryable<T> query, params string[] includeProperties)
    {
        if (includeProperties == null || includeProperties.Length == 0)
            return query;

        foreach (var includeProperty in includeProperties)
        {
            if (!string.IsNullOrWhiteSpace(includeProperty))
            {
                query = query.Include(includeProperty);
            }
        }

        return query;
    }

    /// <summary>
    /// Gets an entity by ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>The entity or null if not found</returns>
    public ValueTask<T?> GetByIdAsync(TKey id)
    {
        return Table.FindAsync(id);
    }

    /// <summary>
    /// Gets an entity by ID with included navigation properties
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <param name="include">The navigation property names to include</param>
    /// <returns>The entity with included navigation properties</returns>
    public async ValueTask<T?> GetByIdAsync(TKey id, params string[] include)
    {
        // First, try to get the entity by its ID
        var entity = await Table.FindAsync(id);

        if (entity == null || include == null || include.Length == 0)
            return entity;

        // Detach the entity to avoid tracking conflicts
        _context.Entry(entity).State = EntityState.Detached;

        // Get the primary key property info
        var keyProperty = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties.FirstOrDefault();
        if (keyProperty == null)
            return entity;

        // Create a predicate to find the entity by its ID
        var parameter = Expression.Parameter(typeof(T), "x");
        var memberAccess = Expression.Property(parameter, keyProperty.Name);
        var constant = Expression.Constant(id, id?.GetType() ?? keyProperty.ClrType);
        var predicate = Expression.Equal(memberAccess, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);

        // Query with includes
        var query = Table.Where(lambda);
        query = ApplyIncludes(query, include);

        // Get the entity with includes
        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets an entity by its key values
    /// </summary>
    /// <param name="keyValues">The key values</param>
    /// <returns>The entity or null if not found</returns>
    public ValueTask<T?> Get(params object?[]? keyValues)
    {
        if (keyValues == null || keyValues.Length == 0)
            return new ValueTask<T?>(result: null);

        return Table.FindAsync(keyValues);
    }

    /// <summary>
    /// Gets an entity by its key values with included navigation properties
    /// </summary>
    /// <param name="keyValues">The key values</param>
    /// <param name="includeProperties">The navigation property names to include</param>
    /// <returns>The entity with included navigation properties</returns>
    public async ValueTask<T?> Get(object?[]? keyValues, params string[] includeProperties)
    {
        if (keyValues == null || keyValues.Length == 0)
            return null;

        // First, try to get the entity by its key values
        var entity = await Table.FindAsync(keyValues);

        if (entity == null || includeProperties == null || includeProperties.Length == 0)
            return entity;

        // Detach the entity to avoid tracking conflicts
        _context.Entry(entity).State = EntityState.Detached;

        // Get the primary key properties
        var keyProperties = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties;
        if (keyProperties == null || keyProperties.Count == 0 || keyProperties.Count != keyValues.Length)
            return entity;

        // Build a predicate to find the entity by its key values
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combinedPredicate = null;

        for (int i = 0; i < keyProperties.Count; i++)
        {
            var property = keyProperties[i];
            var keyValue = keyValues[i];

            var memberAccess = Expression.Property(parameter, property.Name);
            var constant = Expression.Constant(keyValue, keyValue?.GetType() ?? property.ClrType);
            var equalityExpression = Expression.Equal(memberAccess, constant);

            combinedPredicate = combinedPredicate == null
                ? equalityExpression
                : Expression.AndAlso(combinedPredicate, equalityExpression);
        }

        if (combinedPredicate == null)
            return entity;

        var lambda = Expression.Lambda<Func<T, bool>>(combinedPredicate, parameter);

        // Query with includes
        var query = Table.Where(lambda);
        query = ApplyIncludes(query, includeProperties);

        // Get the entity with includes
        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets an entity matching a predicate
    /// </summary>
    /// <param name="predicate">The filter predicate</param>
    /// <returns>The first entity matching the predicate</returns>
    public async Task<T?> Get(Expression<Func<T, bool>> predicate)
    {
        return await Table.FirstOrDefaultAsync(predicate);
    }

    /// <summary>
    /// Gets an entity matching a predicate with included navigation properties
    /// </summary>
    /// <param name="predicate">The filter predicate</param>
    /// <param name="includeProperties">The navigation property names to include</param>
    /// <returns>The first entity matching the predicate with included navigation properties</returns>
    public async Task<T?> Get(Expression<Func<T, bool>> predicate, params string[] includeProperties)
    {
        var query = Table.Where(predicate);
        query = ApplyIncludes(query, includeProperties);
        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets all entities matching a predicate
    /// </summary>
    /// <param name="predicate">The filter predicate</param>
    /// <returns>A list of entities matching the predicate</returns>
    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
    {
        return await Table.Where(predicate).ToListAsync();
    }

    /// <summary>
    /// Gets all entities matching a predicate with included navigation properties
    /// </summary>
    /// <param name="predicate">The filter predicate</param>
    /// <param name="includeProperties">The navigation property names to include</param>
    /// <returns>A list of entities with included navigation properties</returns>
    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate, params string[] includeProperties)
    {
        var query = Table.Where(predicate);
        query = ApplyIncludes(query, includeProperties);
        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets a paged list of entities
    /// </summary>
    /// <param name="predicate">The filter predicate</param>
    /// <param name="orderBy">The order function</param>
    /// <param name="pageNumber">The page number</param>
    /// <param name="pageSize">The page size</param>
    /// <returns>A paged list of entities</returns>
    public async Task<PagedList<T>> GetPagedAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
        int pageNumber = 1,
        int pageSize = 32)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, pageSize);

        var query = Table.Where(predicate);
        var totalCount = await query.CountAsync();
        var orderedQuery = orderBy(query);
        var items = await orderedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedList<T>(items, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Gets a paged list of entities with included navigation properties
    /// </summary>
    /// <param name="predicate">The filter predicate</param>
    /// <param name="orderBy">The order function</param>
    /// <param name="pageNumber">The page number</param>
    /// <param name="pageSize">The page size</param>
    /// <param name="includeProperties">The navigation property names to include</param>
    /// <returns>A paged list of entities with included navigation properties</returns>
    public async Task<PagedList<T>> GetPagedAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
        int pageNumber = 1,
        int pageSize = 32,
        params string[] includeProperties)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, pageSize);

        var query = Table.Where(predicate);
        query = ApplyIncludes(query, includeProperties);

        var totalCount = await query.CountAsync();
        var orderedQuery = orderBy(query);
        var items = await orderedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedList<T>(items, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">The entity to add</param>
    public async ValueTask AddAsync(T entity)
    {
        await Table.AddAsync(entity);
    }

    /// <summary>
    /// Adds multiple entities
    /// </summary>
    /// <param name="entities">The entities to add</param>
    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await Table.AddRangeAsync(entities);
    }

    /// <summary>
    /// Updates an entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    public void Update(T entity)
    {
        Table.Update(entity);
    }

    /// <summary>
    /// Updates an entity with the specified ID using property values from updateData
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <param name="updateData">An object containing the properties to update</param>
    /// <returns>The updated entity</returns>
    public async Task<T> Update(object id, object updateData)
    {
        var entity = new T();
        var keyProperty = (_context.Model.FindEntityType(typeof(T))
                            ?.FindPrimaryKey()
                            ?.Properties[0]) ?? throw new InvalidOperationException($"Entity {typeof(T).Name} does not have a primary key.");
        var keyPropertyName = keyProperty.Name;
        var idProperty = typeof(T).GetProperty(keyPropertyName)
            ?? throw new InvalidOperationException($"Entity {typeof(T).Name} does not have a property named {keyPropertyName}.");

        idProperty.SetValue(entity, id);
        _context.Set<T>().Attach(entity);

        foreach (var property in updateData.GetType().GetProperties())
        {
            var entityProperty = typeof(T).GetProperty(property.Name);
            if (entityProperty != null && property.Name != keyPropertyName)
            {
                var value = property.GetValue(updateData);
                entityProperty.SetValue(entity, value);
                _context.Entry(entity).Property(property.Name).IsModified = true;
            }
        }

        await _context.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Deletes an entity
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    public void DeleteAsync(T entity)
    {
        Table.Remove(entity);
    }

    /// <summary>
    /// Deletes an entity by its ID
    /// </summary>
    /// <param name="key">The entity ID</param>
    public async Task DeleteAsync(TKey key)
    {
        var entity = await Table.FindAsync(key);
        if (entity != null)
        {
            Table.Remove(entity);
        }
    }

    /// <summary>
    /// Deletes entities matching the specified predicate
    /// </summary>
    /// <param name="predicate">The filter predicate</param>
    public Task ExecuteDeleteAsync(Expression<Func<T, bool>> predicate)
    {
        return Table.Where(predicate).ExecuteDeleteAsync();
    }

    /// <summary>
    /// Checks if any entities exist
    /// </summary>
    /// <returns>True if at least one entity exists; otherwise, false</returns>
    public Task<bool> ExistsAsync()
    {
        return Table.AnyAsync();
    }

    /// <summary>
    /// Checks if an entity with the specified ID exists
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>True if the entity exists; otherwise, false</returns>
    public async Task<bool> ExistsAsync(TKey id)
    {
        return await Table.FindAsync(id) != null;
    }

    /// <summary>
    /// Checks if any entities match the specified predicate
    /// </summary>
    /// <param name="predicate">The filter predicate</param>
    /// <returns>True if at least one entity matches; otherwise, false</returns>
    public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return Table.AnyAsync(predicate);
    }

    /// <summary>
    /// Saves all changes made in this repository to the database
    /// </summary>
    public Task SaveAsync()
    {
        return _context.SaveChangesAsync();
    }
}