using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Gargar.Common.Domain.Internal;

/// <summary>
/// Builds and caches property getters.
/// </summary>
public interface IPropertyGetterCache
{
    /// <summary>
    /// Gets a property get for provided <paramref name="property"/>.
    /// </summary>
    /// <param name="property">Property to get the getter for.</param>
    /// <param name="storeObjectIdentifier">Store object identifier</param>
    /// <typeparam name="TEntity">Type of the entity.</typeparam>
    /// <returns>Property getter.</returns>
    Func<DbContext, TEntity, object?> GetPropertyGetter<TEntity>(PropertyWithNavigation property, StoreObjectIdentifier storeObjectIdentifier);
}

/// <summary>
/// Builds and caches property getters.
/// </summary>
public class PropertyGetterCache : IPropertyGetterCache
{
    private readonly ILogger<PropertyGetterCache> _logger;
    private readonly ConcurrentDictionary<PropertyWithNavigation, Delegate> _propertyGetterLookup;

    /// <summary>
    /// Initializes new instance of <see cref="PropertyGetterCache"/>.
    /// </summary>
    /// <param name="loggerFactory">Logger factory.</param>
    public PropertyGetterCache(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<PropertyGetterCache>()
            ?? throw new ArgumentNullException(nameof(loggerFactory));
        _propertyGetterLookup = new ConcurrentDictionary<PropertyWithNavigation, Delegate>();
    }

    /// <inheritdoc />
    public Func<DbContext, TEntity, object?> GetPropertyGetter<TEntity>(PropertyWithNavigation property, StoreObjectIdentifier storeObjectIdentifier)
    {
        return (Func<DbContext, TEntity, object?>)_propertyGetterLookup.GetOrAdd(property, BuildPropertyGetter<TEntity>(property, storeObjectIdentifier));
    }

    private Func<DbContext, TEntity, object?> BuildPropertyGetter<TEntity>(PropertyWithNavigation propertyWithNavigation, StoreObjectIdentifier storeObjectIdentifier)
    {
        var property = propertyWithNavigation.Property;
        var hasSqlDefaultValue = property.GetDefaultValueSql(storeObjectIdentifier) != null;
        var hasDefaultValue = property.TryGetDefaultValue(storeObjectIdentifier, out _);

        if ((hasSqlDefaultValue || hasDefaultValue) && !property.IsNullable)
        {
            if (property.ClrType.IsClass)
            {
                _logger.LogWarning("The corresponding column of '{Entity}.{Property}' has a DEFAULT value constraint in the database and is NOT NULL. Depending on the database vendor the .NET value `null` may lead to an exception because the tool for bulk insert of data may prevent sending `null`s for NOT NULL columns. Use 'PropertiesToInsert/PropertiesToUpdate' on corresponding options to specify properties to insert/update and skip the property so database uses the DEFAULT value.",
                                   property.DeclaringType.ClrType.Name, property.Name);
            }
            else if (!property.ClrType.IsGenericType ||
                     (!property.ClrType.IsGenericTypeDefinition && property.ClrType.GetGenericTypeDefinition() != typeof(Nullable<>)))
            {
                _logger.LogWarning("The corresponding column of '{Entity}.{Property}' has a DEFAULT value constraint in the database and is NOT NULL. Depending on the database vendor the \".NET default values\" (`false`, `0`, `00000000-0000-0000-0000-000000000000` etc.) may lead to unexpected results because these values are sent to the database as-is, i.e. the DEFAULT value constraint will NOT be used by database. Use 'PropertiesToInsert/PropertiesToUpdate' on corresponding options to specify properties to insert and skip the property so database uses the DEFAULT value.",
                                   property.DeclaringType.ClrType.Name, property.Name);
            }
            else
            {
                // No action is taken in this case
            }
        }

        var getter = BuildGetter(property);
        var converter = property.GetValueConverter();

        if (converter != null)
            getter = UseConverter(getter, converter);

        if (propertyWithNavigation.Navigation.Count != 0)
        {
            var naviGetter = BuildNavigationGetter(propertyWithNavigation.Navigation);
            getter = Combine(naviGetter, getter);
        }

        return (Func<DbContext, TEntity, object?>)(object)getter;
    }

    private static Func<object, object?> BuildNavigationGetter(IReadOnlyList<INavigation> navigation)
    {
        Func<object, object?>? getter = null;
        foreach (var t in navigation)
        {
            getter = Combine(getter, t.GetGetter().GetClrValue);
        }

        return getter ?? throw new ArgumentException("No navigation provided.");
    }

    private static Func<object, object?> Combine(Func<object, object?>? parentGetter, Func<object, object?> getter)
    {
        if (parentGetter is null)
            return getter;

        return e =>
        {
            var childEntity = parentGetter(e);

            return childEntity == null ? null : getter(childEntity);
        };
    }

    private static Func<DbContext, object, object?> Combine(Func<object, object?> naviGetter, Func<DbContext, object, object?> getter)
    {
        return (ctx, e) =>
        {
            var childEntity = naviGetter(e);

            return childEntity == null ? null : getter(ctx, childEntity);
        };
    }

    private static Func<DbContext, object, object?> BuildGetter(IProperty property)
    {
        if (property.IsShadowProperty())
            return (ctx, entity) => ctx.Entry(entity).Property(property.Name).CurrentValue;

        var getter = property.GetGetter() ?? throw new ArgumentException($"{nameof(property)} '{property.Name}' of entity '{property.DeclaringType.Name}' has no {nameof(property)} getter.");

        return (_, entity) => getter.GetClrValue(entity);
    }

    private static Func<DbContext, object, object?> UseConverter(Func<DbContext, object, object?> getter, ValueConverter converter)
    {
        var convert = converter.ConvertToProvider;

        return (ctx, e) =>
        {
            var value = getter(ctx, e);

            if (value != null)
                value = convert(value);

            return value;
        };
    }
}