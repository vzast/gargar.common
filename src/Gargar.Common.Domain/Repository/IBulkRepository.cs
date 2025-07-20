using Gargar.Common.Domain.Options;

namespace Gargar.Common.Domain.Repository;

public interface IBulkRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Specifies the types of properties that are supported and not supported.
    ///
    /// <para><b>Supported types:</b></para>
    /// - Entity properties
    /// - Owned entity properties
    /// - Nested owned entity properties
    /// - Shadow properties
    /// - Properties with a value converter
    /// - Properties with a different column name
    ///
    /// <para><b>Not supported types:</b></para>
    /// - Navigation properties
    /// </summary>
    Task BulkInsertAsync(IEnumerable<TEntity> entities,
        Action<BulkInsertOptions<TEntity>>? bulkOptionsConfig = null,
        Action<BulkCopyOptions>? bulkCopyOptionsConfig = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Specifies the types of properties that are supported and not supported.
    ///
    /// <para><b>Supported types:</b></para>
    /// - Entity properties
    /// - Owned entity properties
    /// - Nested owned entity properties
    /// - Shadow properties
    /// - Properties with a value converter
    /// - Properties with a different column name
    ///
    /// <para><b>Not supported types:</b></para>
    /// - Navigation properties
    /// </summary>
    Task BulkUpdateAsync(IEnumerable<TEntity> entities,
        Action<BulkUpdateOptions<TEntity>>? bulkOptionsConfig = null,
        Action<BulkCopyOptions>? bulkCopyOptionsConfig = null,
        CancellationToken cancellationToken = default);

    Task BulkDeleteAsync(IEnumerable<TEntity> entities,
        Action<BulkDeleteOptions<TEntity>>? bulkOptionsConfig = null,
        Action<BulkCopyOptions>? bulkCopyOptionsConfig = null,
        CancellationToken cancellationToken = default);
}