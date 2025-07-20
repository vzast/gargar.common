using Gargar.Common.Domain.Attributes;
using Gargar.Common.Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Gargar.Common.Domain.Extentions;

public static class QueryableExtensions
{
    private static readonly ConcurrentDictionary<(Type Type, string Property), (Type PropertyType, LambdaExpression Lambda)> s_orderByExpressions = new ConcurrentDictionary<(Type, string), (Type, LambdaExpression)>();

    private static readonly ConcurrentDictionary<(Type Type, Type PropertyType, string MethodName), MethodInfo> s_orderByMethods = new ConcurrentDictionary<(Type, Type, string), MethodInfo>();

    [RequiresUnreferencedCode("Type.GetProperty requires unreferenced code.")]
    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, SortingDetails sortingDetails)
    {
        if (sortingDetails?.SortItems == null || sortingDetails.SortItems.Count == 0)
        {
            return source.OrderBy((T x) => 1);
        }

        IOrderedQueryable<T> orderedQueryable = null;
        bool flag = false;
        foreach (SortItem sortItem in sortingDetails.SortItems)
        {
            if (!sortItem.SortBy.IsEmpty())
            {
                if (flag)
                {
                    orderedQueryable = ((sortItem.SortDirection == SortDirection.Ascending) ? orderedQueryable.ThenBy(sortItem.SortBy) : orderedQueryable.ThenByDescending(sortItem.SortBy));
                    continue;
                }

                orderedQueryable = ((sortItem.SortDirection == SortDirection.Ascending) ? source.OrderBy(sortItem.SortBy) : source.OrderByDescending(sortItem.SortBy));
                flag = true;
            }
        }

        return orderedQueryable ?? source.OrderBy((T x) => 1);
    }

    [RequiresUnreferencedCode("Type.GetProperty requires unreferenced code.")]
    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property)
    {
        if (property.IsEmpty())
        {
            return source.OrderBy((T x) => 1);
        }

        return ApplyOrder(source, property, "OrderBy");
    }

    [RequiresUnreferencedCode("Type.GetProperty requires unreferenced code.")]
    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
    {
        if (property.IsEmpty())
        {
            return source.OrderBy((T x) => 1);
        }

        return ApplyOrder(source, property, "OrderByDescending");
    }

    [RequiresUnreferencedCode("Type.GetProperty requires unreferenced code.")]
    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property)
    {
        if (property.IsEmpty())
        {
            return source;
        }

        return ApplyOrder(source, property, "ThenBy");
    }

    [RequiresUnreferencedCode("Type.GetProperty requires unreferenced code.")]
    public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string property)
    {
        if (property.IsEmpty())
        {
            return source;
        }

        return ApplyOrder(source, property, "ThenByDescending");
    }

    [RequiresUnreferencedCode("Type.GetProperty requires unreferenced code.")]
    private static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
    {
        Type typeFromHandle = typeof(T);
        property = property.Trim();
        (Type, LambdaExpression) orderByExpression = GetOrderByExpression(typeFromHandle, property);
        if (orderByExpression.Item1 == null)
        {
            return source.OrderBy((T x) => 1);
        }

        return (IOrderedQueryable<T>)GetOrderByMethod(typeFromHandle, orderByExpression.Item1, methodName).Invoke(null, new object[2] { source, orderByExpression.Item2 });
    }

    [RequiresUnreferencedCode("Type.GetProperty requires unreferenced code.")]
    private static (Type PropertyType, LambdaExpression Lambda) GetOrderByExpression(Type type, string property)
    {
        Type type2 = type;
        if (!s_orderByExpressions.TryGetValue((type, property), out var propertyTypeAndLambda))
        {
            string[] array = property.Split('.');
            ParameterExpression parameterExpression = Expression.Parameter(type, "x");
            Expression expression = parameterExpression;
            string[] array2 = array;
            foreach (string name in array2)
            {
                PropertyInfo property2 = type2.GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                if (property2 == null)
                {
                    return (PropertyType: null, Lambda: null);
                }

                expression = Expression.Property(expression, property2);
                type2 = property2.PropertyType;
            }

            Type delegateType = typeof(Func<,>).MakeGenericType(type, type2);
            propertyTypeAndLambda = (PropertyType: type2, Lambda: Expression.Lambda(delegateType, expression, parameterExpression));
            s_orderByExpressions.AddOrUpdate((type, property), propertyTypeAndLambda, ((Type Type, string Property) tuple, (Type PropertyType, LambdaExpression Lambda) lambdaExpression) => propertyTypeAndLambda);
        }

        return propertyTypeAndLambda;
    }

    [RequiresUnreferencedCode("MethodInfo.MakeGenericMethod requires unreferenced code.")]
    private static MethodInfo GetOrderByMethod(Type type, Type propertyType, string methodName)
    {
        if (!s_orderByMethods.TryGetValue((type, propertyType, methodName), out var methodInfo))
        {
            methodInfo = typeof(Queryable).GetMethods().Single((MethodInfo method) => string.Equals(method.Name, methodName, StringComparison.Ordinal) && method.IsGenericMethodDefinition && method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 2).MakeGenericMethod(type, propertyType);
            s_orderByMethods.AddOrUpdate((type, propertyType, methodName), methodInfo, ((Type Type, Type PropertyType, string MethodName) tuple, MethodInfo info) => methodInfo);
        }

        return methodInfo;
    }

    public static IQueryable<TEntity> ApplyIncludes<TEntity>(this IQueryable<TEntity> source, string[]? relatedProperties, int relatedPropertiesMaxDepth = 3, bool forQuerying = false)
            where TEntity : class
    {
        if (relatedProperties is not { Length: > 0 })
            return source;

        var type = typeof(TEntity);
        var entityRelatedProperties = LoadableRelatedPropertyAttribute.GetRelatedProperties(type, relatedPropertiesMaxDepth);
        if (entityRelatedProperties.Count == 0)
            return source;

        var props = relatedProperties.Contains(LoadRelatedProperties.All[0])
            ? entityRelatedProperties.Keys
            : entityRelatedProperties.Keys.Intersect(relatedProperties);

        foreach (var prop in props)
        {
            var propDetails = entityRelatedProperties[prop];
            if (!forQuerying && propDetails.IsOnlyForQuerying)
            {
                continue;
            }
            source = source.Include(prop);
            if (propDetails.IsSplitQuery)
            {
                source = source.AsSplitQuery();
            }
        }

        return source;
    }

    public static IEnumerable<string> FlattenRelatedProperties(IEnumerable<string> props)
    {
        List<string> list = new List<string>();
        foreach (string prop in props.OrderByDescending((string x) => x))
        {
            if (!list.Exists((string x) => x.StartsWith(prop + ".", StringComparison.OrdinalIgnoreCase)))
            {
                list.Add(prop);
            }
        }

        return list;
    }
}