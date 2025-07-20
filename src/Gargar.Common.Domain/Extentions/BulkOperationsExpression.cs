using System.Linq.Expressions;

namespace Gargar.Common.Domain.Extentions;

public static class BulkOperationsExpression
{
    public static string[] ExtractMembers<T>(this Expression<Func<T, object?>> projection)
    {
        ArgumentNullException.ThrowIfNull(projection);

        var memberNames = new List<string>();

        switch (projection.Body)
        {
            case NewExpression body:
                GetMemberNames(body, memberNames);
                break;

            case MemberExpression:
                memberNames.Add(GetMemberName(projection.Body));
                break;

            default:
                throw new NotSupportedException($"The expression of type '{projection.Body.NodeType}' is not supported. Expression: {projection.Body}.");
        }

        return [.. memberNames];
    }

    private static void GetMemberNames(NewExpression newExpression, List<string> memberNames)
    {
        foreach (var arg in newExpression.Arguments)
        {
            var memberName = GetMemberName(arg);
            if (!string.IsNullOrEmpty(memberName))
                memberNames.Add(memberName);
        }
    }

    private static string GetMemberName(Expression expression)
    {
        if (expression is MemberExpression memberExpr)
        {
            if (memberExpr.Expression is not MemberExpression)
                return memberExpr.Member.Name;

            var parentName = GetMemberName(memberExpr.Expression);
            return string.IsNullOrEmpty(parentName) ? memberExpr.Member.Name : $"{parentName}_{memberExpr.Member.Name}";
        }

        return string.Empty;
    }
}