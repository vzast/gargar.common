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
    /// Gets all entities matching the specified predicate
    /// </summary>
    /// <param name="predicate">The filter expression</param>
    /// <returns>A list of entities</returns>
    public Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate) => 
        Table.Where(predicate).ToListAsync();

    /// <summary>
    /// Gets an entity by its key values
    /// </summary>
    /// <param name="keyValues">The key values</param>
    /// <returns>The entity or null if not found</returns>
    public ValueTask<T?> Get(params object?[]? keyValues) => 
        Table.FindAsync(keyValues);

    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>The entity or null if not found</returns>
    public ValueTask<T?> GetByIdAsync(TKey id) => 
        Table.FindAsync(id);

    /// <summary>
    /// Gets a paged list of entities
    /// </summary>
    /// <param name="predicate">The filter expression</param>
    /// <param name="orderBy">The sort expression</param>
    /// <param name="pageNumber">The page number (1-based)</param>
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
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">The entity to add</param>
    public async ValueTask AddAsync(T entity) => 
        await Table.AddAsync(entity);

    /// <summary>
    /// Adds multiple entities
    /// </summary>
    /// <param name="entities">The entities to add</param>
    public Task AddRangeAsync(IEnumerable<T> entities) => 
        Table.AddRangeAsync(entities);

    /// <summary>
    /// Deletes an entity
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    public void DeleteAsync(T entity) => 
        Table.Remove(entity);

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
    /// <param name="predicate">The filter expression</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public Task ExecuteDeleteAsync(Expression<Func<T, bool>> predicate) => 
        Table.Where(predicate).ExecuteDeleteAsync();

    /// <summary>
    /// Checks if any entities exist
    /// </summary>
    /// <returns>True if at least one entity exists; otherwise, false</returns>
    public Task<bool> ExistsAsync() => 
        Table.AnyAsync();

    /// <summary>
    /// Checks if an entity with the specified ID exists
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>True if the entity exists; otherwise, false</returns>
    public async Task<bool> ExistsAsync(TKey id) => 
        await Table.FindAsync(id) != null;

    /// <summary>
    /// Checks if any entities match the specified predicate
    /// </summary>
    /// <param name="predicate">The filter expression</param>
    /// <returns>True if at least one entity matches; otherwise, false</returns>
    public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) => 
        Table.AnyAsync(predicate);

    /// <summary>
    /// Updates an entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    public void UpdateAsync(T entity) => 
        Table.Update(entity);

    /// <summary>
    /// Updates an entity with the specified ID using property values from updateData
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <param name="updateData">An object containing the properties to update</param>
    /// <returns>The updated entity</returns>
    /// <exception cref="InvalidOperationException">Thrown when primary key cannot be found or set</exception>
    public virtual async Task<T> Update(object id, object updateData)
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
    /// Saves all changes made in this repository to the database
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    public Task SaveAsync() => 
        _context.SaveChangesAsync();
}