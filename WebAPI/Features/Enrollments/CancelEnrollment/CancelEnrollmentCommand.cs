namespace WebAPI.Features.Enrollments.CancelEnrollment;

public sealed record CancelEnrollmentCommand(Guid StudentId, Guid CourseId);
