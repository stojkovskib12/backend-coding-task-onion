using Claims.Application.Abstractions;
using Claims.Domain;
using MediatR;

namespace Claims.Application.Queries.Claims.GetClaim;

public sealed class GetClaimQueryHandler(IClaimsRepository repository) : IRequestHandler<GetClaimQuery, Claim?>
{
    public Task<Claim?> Handle(GetClaimQuery request, CancellationToken cancellationToken) => repository.GetClaimByDisplayIdAsync(request.DisplayId, cancellationToken);
}
