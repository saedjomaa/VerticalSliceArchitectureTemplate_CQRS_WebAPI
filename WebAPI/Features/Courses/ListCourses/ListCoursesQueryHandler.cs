using Microsoft.EntityFrameworkCore;
using WebAPI.Common.Cqrs;
using WebAPI.Common.Pagination;
using WebAPI.Common.Results;
using WebAPI.Database;

namespace WebAPI.Features.Courses.ListCourses;

internal sealed class ListCoursesQueryHandler(SchoolDbContext db)
    : IQueryHandler<ListCoursesQuery, Result<PagedResult<CourseResponse>>>
{
    public async Task<Result<PagedResult<CourseResponse>>> HandleAsync(
        ListCoursesQuery query, CancellationToken cancellationToken)
    {
        var page = await db.Courses.AsNoTracking()
            .OrderBy(course => course.Title)
            .ThenBy(course => course.Id)
            .Select(CourseMappings.ToResponse)
            .ToPageAsync(query.Page, query.PageSize, cancellationToken);

        return Result<PagedResult<CourseResponse>>.Success(page);
    }
}
