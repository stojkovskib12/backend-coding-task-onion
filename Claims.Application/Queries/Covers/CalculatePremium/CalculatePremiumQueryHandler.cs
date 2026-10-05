using Claims.Domain;
using MediatR;

namespace Claims.Application.Queries.Covers.CalculatePremium;

public sealed class CalculatePremiumQueryHandler : IRequestHandler<CalculatePremiumQuery, decimal>
{
    public Task<decimal> Handle(CalculatePremiumQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(PremiumCalculator.Calculate(request.StartDate, request.EndDate, request.Type));
}
