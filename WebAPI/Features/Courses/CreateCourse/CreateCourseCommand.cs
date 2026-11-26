using Domain.Entities;

namespace WebAPI.Features.Courses.CreateCourse;

public sealed record CreateCourseCommand(string Title, int Capacity = Course.DefaultCapacity);
