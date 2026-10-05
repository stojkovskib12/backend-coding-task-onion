using FluentValidation;

namespace Claims.Application.Commands.Claims.DeleteClaim;

public sealed class DeleteClaimCommandValidator : AbstractValidator<DeleteClaimCommand>
{
    public DeleteClaimCommandValidator() => RuleFor(command => command.Id).NotEmpty();
}
