using Microsoft.EntityFrameworkCore;
using WebAPI.Common.Cqrs;
using WebAPI.Common.Results;
using WebAPI.Database;

namespace WebAPI.Features.Students.GetStudent;

internal sealed class GetStudentQueryHandler(SchoolDbContext db)
    : IQueryHandler<GetStudentQuery, Result<StudentResponse>>
{
    public async Task<Result<StudentResponse>> HandleAsync(GetStudentQuery query, CancellationToken cancellationToken)
    {
        var student = await db.Students.AsNoTracking()
            .Where(student => student.Id == query.Id)
            .Select(StudentMappings.ToResponse)
            .SingleOrDefaultAsync(cancellationToken);

        return student is null
            ? Result<StudentResponse>.Failure(StudentErrors.NotFound)
            : Result<StudentResponse>.Success(student);
    }
}
