using FluentValidation;

namespace WebAPI.Features.JsonPlaceholder.GetUser;

internal sealed class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(query => query.Id).GreaterThan(0);
    }
}
