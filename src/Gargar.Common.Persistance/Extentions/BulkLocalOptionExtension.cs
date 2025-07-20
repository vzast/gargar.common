using Gargar.Common.Domain.Options;

namespace Gargar.Common.Persistance.Extentions;

public static class BulkLocalOptionExtension
{
    public static BulkLocalOptions<TEntity> ToBulkLocalOptions<TEntity>(this BulkInsertOptions<TEntity>? options)
        where TEntity : class
    {
        if (options == null) return new BulkLocalOptions<TEntity>();

        return new BulkLocalOptions<TEntity>
        {
            PropertiesToExclude = options.PropertiesToExclude,
            PropertiesToInclude = options.PropertiesToInclude,
            IncludeShadowProperties = options.IncludeShadowProperties
        };
    }

    public static BulkLocalOptions<TEntity> ToBulkLocalOptions<TEntity>(this BulkUpdateOptions<TEntity>? options)
        where TEntity : class
    {
        if (options == null) return new BulkLocalOptions<TEntity>();

        return new BulkLocalOptions<TEntity>
        {
            PropertiesToExclude = options.PropertiesToExclude,
            PropertiesToInclude = options.PropertiesToInclude,
            IncludeShadowProperties = options.IncludeShadowProperties,
            MergeByProperties = options.MergeByProperties
        };
    }

    public static BulkLocalOptions<TEntity> ToBulkLocalOptions<TEntity>(this BulkDeleteOptions<TEntity>? options)
        where TEntity : class
    {
        if (options == null) return new BulkLocalOptions<TEntity>();

        return new BulkLocalOptions<TEntity>
        {
            MergeByProperties = options.MergeByProperties
        };
    }
}