using System.Linq.Expressions;

namespace Gargar.Common.Domain.Options;

public class BulkCopyOptions
{
    /// <summary>
    ///     Number of seconds for the operation to complete before it times out.
    ///     The default is 30 seconds. A value of 0 indicates no limit; the bulk copy will wait indefinitely
    /// </summary>
    public int Timeout { get; set; } = 30;
}

public abstract class BulkOptionsBase
{
    /// <summary>
    ///     Indicates whether to include shadow properties in bulk operations.
    ///     The default is false.
    /// </summary>
    public bool IncludeShadowProperties { get; set; }
}

public class BulkInsertOptions<TEntity> : BulkOptionsBase
    where TEntity : class
{
    /// <summary>
    ///     When doing Insert/Update one or more properties can be excluded by adding their names into PropertiesToExclude.
    /// </summary>
    /// <remarks>
    ///     If you need to change less than half of the columns, you can use PropertiesToInclude. Setting both lists is not allowed.
    /// </remarks>
    public Expression<Func<TEntity, object?>>? PropertiesToExclude { get; set; }

    /// <summary>
    ///     When doing Insert/Update properties to affect can be explicitly selected by adding their names into PropertiesToInclude.
    /// </summary>
    /// <remarks>
    ///     If you need to change more than half of the columns, you can use PropertiesToExclude. Setting both lists is not allowed.
    /// </remarks>
    public Expression<Func<TEntity, object?>>? PropertiesToInclude { get; set; }
}

public class BulkUpdateOptions<TEntity> : BulkInsertOptions<TEntity>
    where TEntity : class
{
    /// <summary>
    ///     Used for specifying custom properties, by which we want update/delete to be done.
    /// </summary>
    /// <remarks>
    ///     If Identity column exists and is not added in MergeByProperties it will be excluded automatically.
    /// </remarks>
    public Expression<Func<TEntity, object?>>? MergeByProperties { get; set; }
}

public class BulkDeleteOptions<TEntity>
    where TEntity : class
{
    /// <summary>
    ///     Used for specifying custom properties, by which we want update/delete to be done.
    /// </summary>
    /// <remarks>
    ///     If Identity column exists and is not added in MergeByProperties it will be excluded automatically.
    /// </remarks>
    public Expression<Func<TEntity, object?>>? MergeByProperties { get; set; }
}