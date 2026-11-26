using FluentValidation;

namespace WebAPI.Features.Enrollments.GetEnrollment;

internal sealed class GetEnrollmentQueryValidator : AbstractValidator<GetEnrollmentQuery>
{
    public GetEnrollmentQueryValidator()
    {
        RuleFor(query => query.StudentId).NotEmpty();
        RuleFor(query => query.CourseId).NotEmpty();
    }
}
