using Microsoft.EntityFrameworkCore;

namespace Gargar.Common.Domain.Helpers;

public static class PagedListHelper
{
    public static async Task<PagedList<TItem>> Create<TItem>(IQueryable<TItem> source, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        int count = await EntityFrameworkQueryableExtensions.CountAsync(source, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        if (count <= 0)
        {
            return new PagedList<TItem>(new List<TItem>(), pageIndex, pageSize, count);
        }

        return new PagedList<TItem>(await EntityFrameworkQueryableExtensions.ToListAsync(source.Skip(pageIndex * pageSize).Take(pageSize), cancellationToken).ConfigureAwait(continueOnCapturedContext: false), pageIndex, pageSize, count);
    }
}