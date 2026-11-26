using Microsoft.EntityFrameworkCore;
using WebAPI.Common.Cqrs;
using WebAPI.Common.Pagination;
using WebAPI.Common.Results;
using WebAPI.Database;

namespace WebAPI.Features.Students.ListStudents;

internal sealed class ListStudentsQueryHandler(SchoolDbContext db)
    : IQueryHandler<ListStudentsQuery, Result<PagedResult<StudentResponse>>>
{
    public async Task<Result<PagedResult<StudentResponse>>> HandleAsync(
        ListStudentsQuery query, CancellationToken cancellationToken)
    {
        var page = await db.Students.AsNoTracking()
            .OrderBy(student => student.FullName)
            .ThenBy(student => student.Id)
            .Select(StudentMappings.ToResponse)
            .ToPageAsync(query.Page, query.PageSize, cancellationToken);

        return Result<PagedResult<StudentResponse>>.Success(page);
    }
}
