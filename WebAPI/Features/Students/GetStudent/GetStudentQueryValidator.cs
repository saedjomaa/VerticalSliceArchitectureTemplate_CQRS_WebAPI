using FluentValidation;

namespace WebAPI.Features.Students.GetStudent;

internal sealed class GetStudentQueryValidator : AbstractValidator<GetStudentQuery>
{
    public GetStudentQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
