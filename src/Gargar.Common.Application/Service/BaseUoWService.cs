using Gargar.Common.Application.Interfaces;
using Gargar.Common.Domain.Helpers;
using Gargar.Common.Domain.Repository;
using System.Linq.Expressions;

namespace Gargar.Common.Application.Service;

/// <summary>
/// Base service implementation for Unit of Work pattern with CRUD operations
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The type of the entity primary key</typeparam>
/// <typeparam name="TDTO">The DTO type used for data transfer</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="BaseUoWService{TEntity, TKey, TDTO}"/> class
/// </remarks>
/// <param name="unitOfWork">The Unit of Work instance</param>
/// <param name="mapper">The mapper service</param>
public abstract class BaseUoWService<TEntity, TKey, TDTO, TMapper>(IUnitOfWork unitOfWork) : IUoWService<TEntity, TKey, TDTO, TMapper> where TMapper : class, IMapper<TEntity, TDTO>, new()
    where TEntity : class, new()
    where TDTO : class
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// The Unit of Work instance
    /// </summary>
    protected readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    /// <summary>
    /// The mapper service for entity-to-DTO conversions
    /// </summary>
    protected readonly IMapper<TEntity, TDTO> _mapper = new TMapper();

    /// <summary>
    /// Gets the repository for the entity type
    /// </summary>
    protected virtual IBaseRepository<TEntity, TKey> Repository => _unitOfWork.Repository<TEntity, TKey>();

    /// <summary>
    /// Adds a new entity to the repository
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added entity converted to DTO</returns>
    public virtual async Task<TDTO> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Repository.AddAsync(entity);
        await _unitOfWork.CommitAsync(cancellationToken);
        return _mapper.Map(entity);
    }

    /// <summary>
    /// Deletes an entity by its ID
    /// </summary>
    /// <param name="id">The ID of the entity to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the entity was found and deleted; otherwise, false</returns>
    public virtual async Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetByIdAsync(id);

        if (entity == null)
            return false;

        Repository.DeleteAsync(entity);
        await _unitOfWork.CommitAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Deletes entities matching the specified predicate
    /// </summary>
    /// <param name="predicate">The filter expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if any entities were deleted; otherwise, false</returns>
    public virtual async Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        bool anyExists = await Repository.ExistsAsync(predicate);

        if (!anyExists)
            return false;

        await Repository.ExecuteDeleteAsync(predicate);
        await _unitOfWork.CommitAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    /// <param name="id">The ID of the entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The entity converted to DTO, or null if not found</returns>
    public virtual async Task<TDTO?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetByIdAsync(id);
        return entity != null ? _mapper.Map(entity) : null;
    }

    /// <summary>
    /// Gets all entities matching the specified predicate
    /// </summary>
    /// <param name="predicate">The filter expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of entities converted to DTOs</returns>
    public virtual async Task<IEnumerable<TDTO>> GetAllAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var entities = await Repository.GetAllAsync(predicate);
        return _mapper.MapCollection(entities);
    }

    /// <summary>
    /// Gets a paged list of entities matching the specified criteria
    /// </summary>
    /// <param name="predicate">The filter expression</param>
    /// <param name="orderBy">The sort expression</param>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">The page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paged list of entities converted to DTOs</returns>
    public virtual async Task<PagedList<TDTO>> GetPagedAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var pagedEntities = await Repository.GetPagedAsync(predicate, orderBy, pageNumber, pageSize);

        var dtoItems = _mapper.MapCollection(pagedEntities.Items);

        return new PagedList<TDTO>(
            [.. dtoItems],
            pagedEntities.TotalCount,
            pagedEntities.PageNumber,
            pagedEntities.PageSize
        );
    }

    /// <summary>
    /// Updates an entity with the specified ID using property values from updateData
    /// </summary>
    /// <param name="id">The ID of the entity to update</param>
    /// <param name="updateData">An object containing the properties to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated entity converted to DTO</returns>
    public virtual async Task<TDTO> UpdateAsync(TKey id, object updateData, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.Update(id, updateData);
        await _unitOfWork.CommitAsync(cancellationToken);
        return _mapper.Map(entity);
    }

    /// <summary>
    /// Checks if an entity with the specified ID exists
    /// </summary>
    /// <param name="id">The ID of the entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the entity exists; otherwise, false</returns>
    public virtual Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return Repository.ExistsAsync(id);
    }

    /// <summary>
    /// Checks if any entities match the specified predicate
    /// </summary>
    /// <param name="predicate">The filter expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if any entities match; otherwise, false</returns>
    public virtual Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return Repository.ExistsAsync(predicate);
    }
}