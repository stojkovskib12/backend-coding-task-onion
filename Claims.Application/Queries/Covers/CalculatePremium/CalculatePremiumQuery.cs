using Claims.Domain;
using MediatR;

namespace Claims.Application.Queries.Covers.CalculatePremium;

public sealed record CalculatePremiumQuery(DateOnly StartDate, DateOnly EndDate, CoverType Type) : IRequest<decimal>;
