using WebAPI.Common.Pagination;

namespace WebAPI.Features.Students.ListStudents;

internal sealed class ListStudentsQueryValidator()
    : PageQueryValidator<ListStudentsQuery>(query => query.Page, query => query.PageSize);
