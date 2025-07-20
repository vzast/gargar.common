using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Gargar.Common.Domain.Extentions;

public static class EntityTypeExtensions
{
    public static string GetTableNameWithSchema(this IEntityType entityType)
    {
        var tableName = entityType.GetTableName();
        var schema = entityType.GetSchema() ?? "dbo";
        return $"[{schema}].[{tableName}]";
    }

    public static IReadOnlyList<IProperty> GetPrimaryKeys(this IEntityType entityType)
    {
        var primaryKey = entityType.FindPrimaryKey();

        if (primaryKey == null || !primaryKey.Properties.Any())
            throw new InvalidOperationException("No primary key defined for the entity");

        return primaryKey.Properties;
    }

    public static StoreObjectIdentifier GetStoreObject(this IEntityType entityType)
    {
        return StoreObjectIdentifier.Create(entityType, StoreObjectType.Table)
            ?? throw new InvalidOperationException($"Could not create StoreObjectIdentifier for table '{entityType.Name}'.");
    }
}