using Gargar.Common.Domain.Extentionsl;
using System.Collections.Concurrent;
using System.Reflection;

namespace Gargar.Common.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class LoadableRelatedPropertyAttribute : Attribute
{
    public bool IgnoreCircularReferenceCheck { get; }
    public bool OnlyForQuerying { get; }
    public bool SplitQuery { get; }

    private static readonly ConcurrentDictionary<Type, Dictionary<string, (bool IsSplitQuery, bool IsOnlyForQuerying)>> s_relatedProperties = new();

    public LoadableRelatedPropertyAttribute(bool ignoreCircularReferenceCheck = false, bool onlyForQuerying = false, bool splitQuery = false)
    {
        IgnoreCircularReferenceCheck = ignoreCircularReferenceCheck;
        OnlyForQuerying = onlyForQuerying;
        SplitQuery = splitQuery;
    }

    public static IReadOnlyDictionary<string, (bool IsSplitQuery, bool IsOnlyForQuerying)> GetRelatedProperties(Type type, int maxDepth = 3)
    {
        return s_relatedProperties.GetOrAdd(type, entityType =>
        {
            var properties = new Dictionary<string, (bool IsSplitQuery, bool IsOnlyForQuerying)>();
            FillRelatedProperties(entityType, null, false, properties, null, maxDepth);
            return properties;
        });
    }

#pragma warning disable S107

    private static void FillRelatedProperties(
        Type type,
        Type? parentType,
        bool ignoreCircularReferenceCheck,
        Dictionary<string, (bool IsSplitQuery, bool IsOnlyForQuerying)> relatedProperties,
        string? prefix,
        int maxDepth,
        int currentDepth = 0,
        bool splitQuery = false)
#pragma warning restore S107
    {
        if (currentDepth > maxDepth)
            return;

        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.GetCustomAttributes<LoadableRelatedPropertyAttribute>().Any())
            .ToList();

        if (props.Count <= 0) return;

        foreach (var prop in props)
        {
            var attribute = prop.GetCustomAttributes<LoadableRelatedPropertyAttribute>().FirstOrDefault();

            if (attribute == null)
                continue;

            var propPath = GetPropertyPath(prefix, prop.Name);
            var propType = prop.PropertyType;

            if (propType.IsAssignableToGenericType(typeof(IEnumerable<>)))
                propType = propType.GetGenericArguments()[0];

            // Check circular references
            if (IsCircularReference(propType, parentType, attribute, ignoreCircularReferenceCheck))
                continue;

            // If at least one property has splitQuery set to true,
            // all other next properties should also be set to true, regardless of whether other properties have SplitQuery set to false
            // It's necessary to quickly check if the requested related properties contain properties with LoadRelatedProperties having splitQuery true. See file #QueryableExtensions:66
            splitQuery = splitQuery || attribute.SplitQuery;

            relatedProperties[propPath] = (splitQuery, attribute.OnlyForQuerying);

            FillRelatedProperties(
                propType,
                type,
                attribute.IgnoreCircularReferenceCheck,
                relatedProperties,
                propPath,
                maxDepth,
                currentDepth + 1,
                splitQuery);
        }
    }

    private static string GetPropertyPath(string? prefix, string propertyName)
        => prefix == null ? propertyName : $"{prefix}.{propertyName}";

    private static bool IsCircularReference(Type propType, Type? parentType, LoadableRelatedPropertyAttribute attribute, bool ignoreCircularReferenceCheck)
        => !attribute.IgnoreCircularReferenceCheck && !ignoreCircularReferenceCheck && propType == parentType;
}