using System.Linq.Expressions;
using Domain.Entities;

namespace WebAPI.Features.Courses;

internal static class CourseMappings
{
    public static readonly Expression<Func<Course, CourseResponse>> ToResponse = course =>
        new CourseResponse(course.Id, course.Title, course.Capacity, course.EnrolledCount, course.RowVersion);
}
