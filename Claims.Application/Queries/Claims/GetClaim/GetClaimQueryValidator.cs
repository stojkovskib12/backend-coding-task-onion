using FluentValidation;

namespace Claims.Application.Queries.Claims.GetClaim;

public sealed class GetClaimQueryValidator : AbstractValidator<GetClaimQuery>
{
    public GetClaimQueryValidator() => RuleFor(query => query.DisplayId).GreaterThan(0);
}
