using Claims.Application.Abstractions;
using FluentValidation;

namespace Claims.Application.Commands.Covers.CreateCover;

public sealed class CreateCoverCommandValidator : AbstractValidator<CreateCoverCommand>
{
    public CreateCoverCommandValidator(ISystemClock clock)
    {
        RuleFor(command => command.StartDate).Must(date => date >= clock.Today)
            .WithMessage("Cover start date cannot be in the past.");
        RuleFor(command => command.EndDate).Must((command, endDate) => endDate > command.StartDate)
            .WithMessage("Cover end date must be after its start date.");
        RuleFor(command => command.EndDate).Must((command, endDate) => command.StartDate.Year == 9999 || endDate <= command.StartDate.AddYears(1))
            .WithMessage("Cover period cannot exceed one year.");
    }
}
