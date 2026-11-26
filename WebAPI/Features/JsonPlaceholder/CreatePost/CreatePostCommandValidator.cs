using FluentValidation;

namespace WebAPI.Features.JsonPlaceholder.CreatePost;

internal sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(command => command.UserId).GreaterThan(0);
        RuleFor(command => command.Title).NotEmpty().MaximumLength(CreatePostCommand.MaximumTitleLength);
        RuleFor(command => command.Body).NotEmpty().MaximumLength(CreatePostCommand.MaximumBodyLength);
    }
}
