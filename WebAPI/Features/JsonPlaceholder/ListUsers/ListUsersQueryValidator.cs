using WebAPI.Common.Pagination;

namespace WebAPI.Features.JsonPlaceholder.ListUsers;

internal sealed class ListUsersQueryValidator()
    : PageQueryValidator<ListUsersQuery>(query => query.Page, query => query.PageSize);
