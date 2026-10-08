using FluentValidation;
using Claims.Application.Abstractions;

namespace Claims.Application.Commands.Covers.DeleteCover;

public sealed class DeleteCoverCommandValidator : AbstractValidator<DeleteCoverCommand>
{
    public DeleteCoverCommandValidator(IClaimsRepository repository)
    {
        RuleFor(command => command.DisplayId).GreaterThan(0);
        RuleFor(command => command).CustomAsync(async (command, context, cancellationToken) =>
        {
            if (command.DisplayId > 0 && await repository.GetCoverByDisplayIdAsync(command.DisplayId, cancellationToken) is { } cover && await repository.HasClaimsForCoverAsync(cover.Id, cancellationToken))
                context.AddFailure(nameof(command.DisplayId), "A cover with claims cannot be deleted.");
        });
    }
}
