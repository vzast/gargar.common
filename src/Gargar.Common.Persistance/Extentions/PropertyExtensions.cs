using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Gargar.Common.Persistance.Extentions;

public static class PropertyExtensions
{
    public static IEnumerable<string> GetColumnNames(this IEnumerable<IProperty> properties, IEntityType entityType)
    {
        var storeObject = entityType.GetStoreObject();

        return properties.Select(property => property.GetColumnName(storeObject) ?? property.Name);
    }
}