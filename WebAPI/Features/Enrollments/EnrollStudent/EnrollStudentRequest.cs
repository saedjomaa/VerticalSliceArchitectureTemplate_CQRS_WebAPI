namespace WebAPI.Features.Enrollments.EnrollStudent;

/// <summary>Request body of POST /api/students/{studentId}/enrollments; the student id comes from the route.</summary>
public sealed record EnrollStudentRequest(Guid CourseId);
