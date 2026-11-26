using WebAPI.Common.Pagination;

namespace WebAPI.Features.Students.ListStudents;

public sealed record ListStudentsQuery(int Page = Pagination.DefaultPage, int PageSize = Pagination.DefaultPageSize);
