using Gargar.Common.Domain.Extentions;
using Gargar.Common.Domain.Internal;
using Gargar.Common.Domain.Options;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Gargar.Common.Domain.Repository;

public interface IEntityPropertiesProvider
{
    IReadOnlyList<PropertyWithNavigation> GetPropertiesForInsert<TEntity>(IEntityType entityType, BulkLocalOptions<TEntity> bulkOptions) where TEntity : class;

    IReadOnlyList<PropertyWithNavigation> GetPropertiesForUpdate<TEntity>(IEntityType entityType, BulkLocalOptions<TEntity> bulkOptions) where TEntity : class;

    IReadOnlyList<PropertyWithNavigation> GetPrimaryKeysOrPropertiesForMerge<TEntity>(IEntityType entityType, BulkLocalOptions<TEntity> bulkOptions) where TEntity : class;
}

public class EntityPropertiesProvider : IEntityPropertiesProvider
{
    #region public members

    public IReadOnlyList<PropertyWithNavigation> GetPropertiesForInsert<TEntity>(IEntityType entityType, BulkLocalOptions<TEntity> bulkOptions)
        where TEntity : class
        => GetProperties(entityType, bulkOptions, []);

    public IReadOnlyList<PropertyWithNavigation> GetPropertiesForUpdate<TEntity>(IEntityType entityType, BulkLocalOptions<TEntity> bulkOptions)
        where TEntity : class
    {
        var mergeByProperties = bulkOptions.MergeByProperties == null
          ? entityType.GetPrimaryKeys().Select(x => x.Name).ToArray()
          : bulkOptions.MergeByProperties.ExtractMembers();

        return GetProperties(entityType, bulkOptions, mergeByProperties);
    }

    public IReadOnlyList<PropertyWithNavigation> GetPrimaryKeysOrPropertiesForMerge<TEntity>(IEntityType entityType, BulkLocalOptions<TEntity> bulkOptions)
        where TEntity : class
    {
        if (bulkOptions.MergeByProperties == null)
            return entityType.GetPrimaryKeys().Select(x => new PropertyWithNavigation(x, x.Name, [])).ToList();

        var members = bulkOptions.MergeByProperties.ExtractMembers();
        Func<IProperty, string, bool> predicate = (p, n) => members.Contains(n);
        return GetAllProperties(entityType, predicate);
    }

    #endregion public members

    #region private members

    private static List<PropertyWithNavigation> GetProperties<TEntity>(IEntityType entityType, BulkLocalOptions<TEntity> bulkOptions, string[] mergeByProperties)
       where TEntity : class
    {
        if (bulkOptions.PropertiesToInclude != null && bulkOptions.PropertiesToExclude != null)
            throw new InvalidOperationException($"Setting both {bulkOptions.PropertiesToInclude} and {bulkOptions.PropertiesToExclude} are not allowed");

        Func<IProperty, string, bool> shadowPredicate = (p, n) => mergeByProperties.Contains(n) || (bulkOptions.IncludeShadowProperties && p.IsShadowProperty());

        if (bulkOptions.PropertiesToInclude != null)
        {
            var members = bulkOptions.PropertiesToInclude.ExtractMembers();
            Func<IProperty, string, bool> predicate = (p, n) => members.Contains(n) || shadowPredicate(p, n);
            return GetAllProperties(entityType, predicate);
        }

        if (bulkOptions.PropertiesToExclude != null)
        {
            var members = bulkOptions.PropertiesToExclude.ExtractMembers();
            Func<IProperty, string, bool> predicate = (p, n) => (!members.Contains(n) && !p.IsShadowProperty()) || shadowPredicate(p, n);
            return GetAllProperties(entityType, predicate);
        }

        return GetAllProperties(entityType, (p, n) => bulkOptions.IncludeShadowProperties || !p.IsShadowProperty());
    }

    private static List<PropertyWithNavigation> GetAllProperties(IEntityType entityType, Func<IProperty, string, bool> predicate)
    {
        var properties = new List<PropertyWithNavigation>();

        var propertiesToAdd = entityType.GetProperties()
            .Where(x => predicate(x, x.Name))
            .Select(x => new PropertyWithNavigation(x, x.Name, []));

        properties.AddRange(propertiesToAdd);

        var entityNavigationOwned = entityType
         .GetNavigations()
         .Where(a => a.IsInlined());

        foreach (var ownedNavigation in entityNavigationOwned)
            AddProperties(ownedNavigation.TargetEntityType, ownedNavigation.Name, [ownedNavigation], properties, predicate);

        return properties;
    }

    private static void AddProperties(IEntityType entityType, string parentPropertyName, IReadOnlyList<INavigation> navigation, List<PropertyWithNavigation> properties, Func<IProperty, string, bool> predicate)
    {
        var propertiesToAdd = entityType.GetProperties()
            .Where(x => !x.IsKey() && predicate(x, $"{parentPropertyName}_{x.Name}"))
            .Select(x => new PropertyWithNavigation(x, $"{parentPropertyName}_{x.Name}", navigation));

        properties.AddRange(propertiesToAdd);

        var entityNavigationOwned = entityType
         .GetNavigations()
         .Where(a => a.IsInlined());

        foreach (var ownedNavigation in entityNavigationOwned)
        {
            var innerNavigationList = navigation.ToList();
            innerNavigationList.Add(ownedNavigation);
            AddProperties(ownedNavigation.TargetEntityType, $"{parentPropertyName}_{ownedNavigation.Name}", innerNavigationList, properties, predicate);
        }
    }

    #endregion private members
}