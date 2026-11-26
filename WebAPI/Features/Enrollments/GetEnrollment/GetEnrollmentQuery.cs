namespace WebAPI.Features.Enrollments.GetEnrollment;

public sealed record GetEnrollmentQuery(Guid StudentId, Guid CourseId);
