using Hirfolyam.Application.Shared;
using Microsoft.EntityFrameworkCore;

namespace Hirfolyam.Application.Extensions;

public static class QueryableExtension
{
    // The query must be ordered, otherwise the pages can overlap.
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var totalItems = await query.CountAsync(cancellationToken);
        // The query validators keep the offset within the int range.
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }
}
