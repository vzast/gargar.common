namespace Gargar.Common.Domain.Helpers;

public static class Guard
{
    public static void NotNull(object arg, string argName, string message = null)
    {
        if (arg == null)
        {
            throw new ArgumentNullException(argName, message);
        }
    }

    public static void NotEmpty(string arg, string argName, string message = null)
    {
        if (string.IsNullOrEmpty(arg))
        {
            throw new ArgumentException(argName, message);
        }
    }

    public static void NotEmpty<T>(ICollection<T> arg, string argName, string message = null)
    {
        if (arg == null || arg.Count == 0)
        {
            throw new ArgumentException(argName, message);
        }
    }

    public static void NotEmpty(Guid arg, string argName, string message = null)
    {
        if (arg == Guid.Empty)
        {
            throw new ArgumentException(argName, message);
        }
    }

    public static void InRange<T>(T arg, T min, T max, string argName, string message = null) where T : struct, IComparable<T>
    {
        if (arg.CompareTo(min) < 0 || arg.CompareTo(max) > 0)
        {
            throw new ArgumentOutOfRangeException(argName, message);
        }
    }

    public static void NotOutOfLength(string arg, int maxLength, string argName, string message = null)
    {
        if (arg.Trim().Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(argName, message);
        }
    }

    public static void NotNegative<T>(T arg, string argName, string message = null) where T : struct, IComparable<T>
    {
        if (arg.CompareTo(default(T)) < 0)
        {
            throw new ArgumentOutOfRangeException(argName, message);
        }
    }

    public static void NotZero<T>(T arg, string argName, string message = null) where T : struct, IComparable<T>
    {
        if (arg.CompareTo(default(T)) == 0)
        {
            throw new ArgumentOutOfRangeException(argName, message);
        }
    }

    public static void NotEqual<T>(T arg1, T arg2, string message = null) where T : IComparable<T>
    {
        if (arg1.CompareTo(arg2) != 0)
        {
            throw new ArgumentException($"{arg1} {arg2}", message);
        }
    }

    public static void LessThan<T>(T arg, T maxValue, string argName) where T : IComparable<T>
    {
        if (arg.CompareTo(maxValue) <= 0)
        {
            throw new ArgumentException($"{argName} {maxValue}", "cannot exceed");
        }
    }

    public static void GreaterThan<T>(T arg, T minValue, string argNameName) where T : IComparable<T>
    {
        if (arg.CompareTo(minValue) >= 0)
        {
            throw new ArgumentException($"{argNameName} {minValue}", " cannot be lower than");
        }
    }

    public static void Against(Func<bool> predicate, string message)
    {
        if (predicate())
        {
            throw new ArgumentException(message);
        }
    }

    public static void InheritsFrom<TBase>(Type type, string message = null)
    {
        if (type.BaseType != typeof(TBase))
        {
            throw new ArgumentException(message ?? $"{type.BaseType.Name} is not {typeof(TBase)}");
        }
    }

    public static void InheritsFrom<TBase>(object instance, string message = null) where TBase : Type
    {
        InheritsFrom<TBase>(instance.GetType(), message);
    }

    public static void Implements<TInterface>(Type type, string message = null)
    {
        if (!typeof(TInterface).IsAssignableFrom(type))
        {
            throw new NotImplementedException(message ?? $"{type.Name} does not implement {typeof(TInterface)} interface.");
        }
    }

    public static void Implements<TInterface>(object instance, string message = null)
    {
        Implements<TInterface>(instance.GetType(), message);
    }

    public static void TypeOf<TType>(object instance, string message = null)
    {
        if (!(instance is TType))
        {
            throw new Exception(message ?? $"{instance.GetType().Name} is not {typeof(TType)}");
        }
    }
}