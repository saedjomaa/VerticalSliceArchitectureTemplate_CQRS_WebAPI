using WebAPI.Common.Pagination;

namespace WebAPI.Features.Courses.ListCourses;

internal sealed class ListCoursesQueryValidator()
    : PageQueryValidator<ListCoursesQuery>(query => query.Page, query => query.PageSize);
