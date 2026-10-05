using FluentValidation;

namespace Claims.Application.Queries.Covers.CalculatePremium;

public sealed class CalculatePremiumQueryValidator : AbstractValidator<CalculatePremiumQuery>
{
    public CalculatePremiumQueryValidator()
    {
        RuleFor(query => query.EndDate).Must((query, endDate) => endDate >= query.StartDate)
            .WithMessage("End date cannot be before start date.");
    }
}
