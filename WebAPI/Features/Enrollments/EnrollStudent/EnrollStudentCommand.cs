namespace WebAPI.Features.Enrollments.EnrollStudent;

public sealed record EnrollStudentCommand(Guid StudentId, Guid CourseId);
