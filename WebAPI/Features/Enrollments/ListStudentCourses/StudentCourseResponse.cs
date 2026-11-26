namespace WebAPI.Features.Enrollments.ListStudentCourses;

public sealed record StudentCourseResponse(Guid CourseId, string Title, DateTimeOffset EnrolledAt);
