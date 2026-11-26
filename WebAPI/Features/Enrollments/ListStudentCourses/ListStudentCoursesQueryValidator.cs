using FluentValidation;
using WebAPI.Common.Pagination;

namespace WebAPI.Features.Enrollments.ListStudentCourses;

internal sealed class ListStudentCoursesQueryValidator : PageQueryValidator<ListStudentCoursesQuery>
{
    public ListStudentCoursesQueryValidator() : base(query => query.Page, query => query.PageSize)
    {
        RuleFor(query => query.StudentId).NotEmpty();
    }
}
