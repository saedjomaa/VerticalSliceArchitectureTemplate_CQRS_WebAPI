using FluentValidation;

namespace WebAPI.Features.Courses.GetCourse;

internal sealed class GetCourseQueryValidator : AbstractValidator<GetCourseQuery>
{
    public GetCourseQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
