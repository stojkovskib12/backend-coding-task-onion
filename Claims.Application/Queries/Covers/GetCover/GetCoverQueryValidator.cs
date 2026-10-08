using FluentValidation;

namespace Claims.Application.Queries.Covers.GetCover;

public sealed class GetCoverQueryValidator : AbstractValidator<GetCoverQuery>
{
    public GetCoverQueryValidator() => RuleFor(query => query.DisplayId).GreaterThan(0);
}
