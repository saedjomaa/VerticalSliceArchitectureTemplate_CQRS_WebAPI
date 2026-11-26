using Domain.Entities;
using WebAPI.Common.Cqrs;
using WebAPI.Common.Results;
using WebAPI.Database;

namespace WebAPI.Features.Students.CreateStudent;

internal sealed class CreateStudentCommandHandler(SchoolDbContext db)
    : ICommandHandler<CreateStudentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateStudentCommand command, CancellationToken cancellationToken)
    {
        var student = Student.Create(command.FullName);
        db.Students.Add(student);
        await db.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(student.Id);
    }
}
