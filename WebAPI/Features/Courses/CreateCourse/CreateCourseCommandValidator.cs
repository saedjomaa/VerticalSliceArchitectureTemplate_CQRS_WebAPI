using Domain.Entities;
using FluentValidation;

namespace WebAPI.Features.Courses.CreateCourse;

internal sealed class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(command => command.Title).NotEmpty().MaximumLength(Course.MaximumTitleLength);
        RuleFor(command => command.Capacity).GreaterThan(0);
    }
}
