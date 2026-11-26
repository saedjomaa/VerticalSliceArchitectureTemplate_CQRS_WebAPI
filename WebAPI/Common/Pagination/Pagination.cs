using Microsoft.EntityFrameworkCore;
using WebAPI.Common.Results;

namespace WebAPI.Common.Pagination;

public static class Pagination
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaximumPageSize = 100;

    internal static async Task<PagedResult<T>> ToPageAsync<T>(
        this IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var offset = (page - 1L) * pageSize;
        if (offset >= totalCount)
        {
            return new PagedResult<T>([], page, pageSize, totalCount);
        }

        var items = await query.Skip((int)offset).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<T>(items, page, pageSize, totalCount);
    }
}
