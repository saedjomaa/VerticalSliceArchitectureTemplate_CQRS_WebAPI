using Microsoft.EntityFrameworkCore;
using WebAPI.Common.Cqrs;
using WebAPI.Common.Results;
using WebAPI.Database;

namespace WebAPI.Features.Courses.GetCourse;

internal sealed class GetCourseQueryHandler(SchoolDbContext db)
    : IQueryHandler<GetCourseQuery, Result<CourseResponse>>
{
    public async Task<Result<CourseResponse>> HandleAsync(GetCourseQuery query, CancellationToken cancellationToken)
    {
        var course = await db.Courses.AsNoTracking()
            .Where(course => course.Id == query.Id)
            .Select(CourseMappings.ToResponse)
            .SingleOrDefaultAsync(cancellationToken);

        return course is null
            ? Result<CourseResponse>.Failure(CourseErrors.NotFound)
            : Result<CourseResponse>.Success(course);
    }
}
