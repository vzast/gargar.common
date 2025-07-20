using Microsoft.EntityFrameworkCore;

namespace Gargar.Common.Persistance.Database;

internal static class DbContexts
{
    private static readonly List<Type> s_contextTypes = new();

    internal static void AddContextType<TContext>()
        where TContext : DbContext
    {
        var contextType = typeof(TContext);
        if (s_contextTypes.Contains(contextType))
            return;

        s_contextTypes.Add(contextType);
    }

    internal static Type[] GetContextTypes => s_contextTypes.ToArray();
}