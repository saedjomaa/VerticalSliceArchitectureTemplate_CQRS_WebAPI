namespace WebAPI.Features.Enrollments.GetEnrollment;

public sealed record EnrollmentResponse(Guid StudentId, Guid CourseId, DateTimeOffset EnrolledAt);
