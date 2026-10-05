using FluentValidation;

namespace Claims.Application.Commands.Covers.DeleteCover;

public sealed class DeleteCoverCommandValidator : AbstractValidator<DeleteCoverCommand>
{
    public DeleteCoverCommandValidator() => RuleFor(command => command.Id).NotEmpty();
}
