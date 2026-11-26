using WebAPI.Common.Pagination;

namespace WebAPI.Features.JsonPlaceholder.ListUsers;

public sealed record ListUsersQuery(int Page = Pagination.DefaultPage, int PageSize = Pagination.DefaultPageSize);
