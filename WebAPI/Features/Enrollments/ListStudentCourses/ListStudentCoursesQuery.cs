using WebAPI.Common.Pagination;

namespace WebAPI.Features.Enrollments.ListStudentCourses;

public sealed record ListStudentCoursesQuery(
    Guid StudentId, int Page = Pagination.DefaultPage, int PageSize = Pagination.DefaultPageSize);
