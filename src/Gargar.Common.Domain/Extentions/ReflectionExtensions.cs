using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Gargar.Common.Domain.Extentionsl;

public static class ReflectionExtensions
{
    [RequiresUnreferencedCode("Type.GetInterfaces requires unreferenced code.")]
    public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
    {
        if (givenType.GetTypeInfo().IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
        {
            return true;
        }

        Type[] interfaces = givenType.GetInterfaces();
        foreach (Type type in interfaces)
        {
            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }
        }

        if (givenType.GetTypeInfo().BaseType == null)
        {
            return false;
        }

        return givenType.GetTypeInfo().BaseType.IsAssignableToGenericType(genericType);
    }
}