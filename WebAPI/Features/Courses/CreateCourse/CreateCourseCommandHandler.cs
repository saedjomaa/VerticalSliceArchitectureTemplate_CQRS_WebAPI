using Domain.Entities;
using WebAPI.Common.Cqrs;
using WebAPI.Common.Results;
using WebAPI.Database;

namespace WebAPI.Features.Courses.CreateCourse;

internal sealed class CreateCourseCommandHandler(SchoolDbContext db)
    : ICommandHandler<CreateCourseCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateCourseCommand command, CancellationToken cancellationToken)
    {
        var course = Course.Create(command.Title, command.Capacity);
        db.Courses.Add(course);
        await db.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(course.Id);
    }
}
