using WebAPI.Common.Pagination;

namespace WebAPI.Features.Courses.ListCourses;

public sealed record ListCoursesQuery(int Page = Pagination.DefaultPage, int PageSize = Pagination.DefaultPageSize);
