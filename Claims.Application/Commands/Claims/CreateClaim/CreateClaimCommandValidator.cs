using Claims.Application.Abstractions;
using FluentValidation;

namespace Claims.Application.Commands.Claims.CreateClaim;

public sealed class CreateClaimCommandValidator : AbstractValidator<CreateClaimCommand>
{
    public CreateClaimCommandValidator(IClaimsRepository repository)
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.DamageCost).InclusiveBetween(0m, 100_000m);
        RuleFor(command => command.CoverId).NotEmpty();
        RuleFor(command => command).CustomAsync(async (command, context, cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(command.CoverId)) return;
            var cover = await repository.GetCoverAsync(command.CoverId, cancellationToken);
            if (cover is null)
            {
                context.AddFailure(nameof(command.CoverId), "The related cover does not exist.");
                return;
            }

            if (command.Created < cover.StartDate || command.Created > cover.EndDate)
                context.AddFailure(nameof(command.Created), "Claim created date must fall within the cover period.");
        });
    }
}
