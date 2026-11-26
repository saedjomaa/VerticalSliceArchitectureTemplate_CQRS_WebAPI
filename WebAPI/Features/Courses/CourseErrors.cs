using WebAPI.Common.Results;

namespace WebAPI.Features.Courses;

internal static class CourseErrors
{
    public static readonly Error NotFound = new(
        "Courses.NotFound", "The course was not found.", ErrorType.NotFound);
}
