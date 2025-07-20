using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Data;

namespace Gargar.Common.Domain.Internal;

/// <summary>
/// Data reader for Entity Framework Core entities.
/// </summary>
/// <typeparam name="TEntity">Type of the entity.</typeparam>
public sealed class EntityDataReader<TEntity> : IDataReader
{
    private readonly DbContext _ctx;
    private readonly IEnumerator<TEntity> _enumerator;
    private readonly Func<DbContext, TEntity, object?>[] _propertyGetterLookup;

    /// <inheritdoc />
    private readonly IReadOnlyList<PropertyWithNavigation> _properties;

    /// <inheritdoc />
    public int FieldCount => _properties.Count;

    /// <summary>
    /// Initializes <see cref="EntityDataReader{T}"/>
    /// </summary>
    /// <param name="ctx">Database context.</param>
    /// <param name="propertyGetterCache">Property getter cache.</param>
    /// <param name="entities">Entities to read.</param>
    /// <param name="properties">Properties to read.</param>
    /// <param name="storeObjectIdentifier">Store Object Identifier</param>
    public EntityDataReader(
       DbContext ctx,
       IPropertyGetterCache propertyGetterCache,
       IEnumerable<TEntity> entities,
       IReadOnlyList<PropertyWithNavigation> properties,
       StoreObjectIdentifier storeObjectIdentifier)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(propertyGetterCache);
        ArgumentNullException.ThrowIfNull(entities);
        ArgumentNullException.ThrowIfNull(properties);

        _ctx = ctx;
        _properties = properties;

        if (properties.Count == 0)
            throw new ArgumentException("The properties collection cannot be empty.", nameof(properties));

        _propertyGetterLookup = BuildPropertyGetterLookup(propertyGetterCache, properties, storeObjectIdentifier);
        _enumerator = entities.GetEnumerator();
    }

    private static Func<DbContext, TEntity, object?>[] BuildPropertyGetterLookup(
       IPropertyGetterCache propertyGetterCache,
       IReadOnlyList<PropertyWithNavigation> properties,
       StoreObjectIdentifier storeObjectIdentifier)
    {
        var lookup = new Func<DbContext, TEntity, object?>[properties.Count];

        for (var i = 0; i < properties.Count; i++)
        {
            lookup[i] = propertyGetterCache.GetPropertyGetter<TEntity>(properties[i], storeObjectIdentifier);
        }

        return lookup;
    }

#pragma warning disable CS8766

    /// <inheritdoc />
    public object? GetValue(int i)
    {
        return _propertyGetterLookup[i](_ctx, _enumerator.Current);
    }

#pragma warning restore CS8766

    /// <inheritdoc />
    public bool Read() => _enumerator.MoveNext();

    public bool IsDBNull(int i) => false;

    /// <inheritdoc />
    public void Dispose()
    {
        _enumerator.Dispose();
    }

    // The following methods are not needed for bulk insert.
    // ReSharper disable ArrangeMethodOrOperatorBody
    object IDataRecord.this[int i] => throw new NotSupportedException();

    object IDataRecord.this[string name] => throw new NotSupportedException();
    int IDataReader.Depth => throw new NotSupportedException();
    int IDataReader.RecordsAffected => throw new NotSupportedException();
    bool IDataReader.IsClosed => throw new NotSupportedException();

    void IDataReader.Close() => throw new NotSupportedException();

    string IDataRecord.GetName(int i) => throw new NotSupportedException();

    string IDataRecord.GetDataTypeName(int i) => throw new NotSupportedException();

    Type IDataRecord.GetFieldType(int i) => throw new NotSupportedException();

    int IDataRecord.GetValues(object[] values) => throw new NotSupportedException();

    int IDataRecord.GetOrdinal(string name) => throw new NotSupportedException();

    bool IDataRecord.GetBoolean(int i) => throw new NotSupportedException();

    byte IDataRecord.GetByte(int i) => throw new NotSupportedException();

    long IDataRecord.GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) => throw new NotSupportedException();

    char IDataRecord.GetChar(int i) => throw new NotSupportedException();

    long IDataRecord.GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) => throw new NotSupportedException();

    Guid IDataRecord.GetGuid(int i) => throw new NotSupportedException();

    short IDataRecord.GetInt16(int i) => throw new NotSupportedException();

    int IDataRecord.GetInt32(int i) => throw new NotSupportedException();

    long IDataRecord.GetInt64(int i) => throw new NotSupportedException();

    float IDataRecord.GetFloat(int i) => throw new NotSupportedException();

    double IDataRecord.GetDouble(int i) => throw new NotSupportedException();

    string IDataRecord.GetString(int i) => throw new NotSupportedException();

    decimal IDataRecord.GetDecimal(int i) => throw new NotSupportedException();

    DateTime IDataRecord.GetDateTime(int i) => throw new NotSupportedException();

    IDataReader IDataRecord.GetData(int i) => throw new NotSupportedException();

    DataTable IDataReader.GetSchemaTable() => throw new NotSupportedException();

    bool IDataReader.NextResult() => throw new NotSupportedException();
}