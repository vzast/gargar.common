using Microsoft.EntityFrameworkCore.Metadata;

namespace Gargar.Common.Domain.Internal;

public readonly struct PropertyWithNavigation : IEquatable<PropertyWithNavigation>
{
    public IProperty Property { get; }
    public IReadOnlyList<INavigation> Navigation { get; }
    public string PropertyName { get; }

    public PropertyWithNavigation(
        IProperty property,
        string propertyName,
        IReadOnlyList<INavigation> navigation)
    {
        Property = property;
        Navigation = navigation;
        PropertyName = propertyName;
    }

    public override bool Equals(object? obj)
    {
        return obj is PropertyWithNavigation other && Equals(other);
    }

    /// <inheritdoc />
    public bool Equals(PropertyWithNavigation other)
    {
        return Property.Equals(other.Property) && Equals(other.Navigation) && PropertyName.Equals(other.PropertyName);
    }

    private bool Equals(IReadOnlyList<INavigation> other)
    {
        if (Navigation.Count != other.Count)
            return false;

        for (var i = 0; i < Navigation.Count; i++)
        {
            var navigation = Navigation[i];
            var otherNavi = other[i];

            if (!navigation.Equals(otherNavi))
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Property);
        ComputeHashCode(Navigation, hashCode);

        return hashCode.ToHashCode();
    }

    /// <summary>
    /// Computes the hashcode for navigation.
    /// </summary>
    /// <param name="navigation">Navigation to compute hashcode for.</param>
    /// <param name="hashCode">Instance of <see cref="HashCode"/> to use.</param>
    public static void ComputeHashCode(IEnumerable<INavigation> navigation, HashCode hashCode)
    {
        foreach (var nav in navigation)
        {
            hashCode.Add(nav);
        }
    }

    public static bool operator ==(PropertyWithNavigation left, PropertyWithNavigation right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PropertyWithNavigation left, PropertyWithNavigation right)
    {
        return !(left == right);
    }
}