using System.Linq.Expressions;

namespace Gargar.Common.Domain.Options;

public class BulkLocalOptions<TEntity>
        where TEntity : class
{
    public Expression<Func<TEntity, object?>>? PropertiesToExclude { get; set; }

    public Expression<Func<TEntity, object?>>? PropertiesToInclude { get; set; }

    public Expression<Func<TEntity, object?>>? MergeByProperties { get; set; }

    public bool IncludeShadowProperties { get; set; }
}