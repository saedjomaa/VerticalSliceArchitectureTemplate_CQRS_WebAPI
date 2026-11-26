namespace WebAPI.Features.Courses;

public sealed record CourseResponse(Guid Id, string Title, int Capacity, int EnrolledCount, Guid RowVersion);
