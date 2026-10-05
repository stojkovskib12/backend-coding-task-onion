using Claims.Application.Abstractions;
using Claims.Domain;
using MediatR;

namespace Claims.Application.Queries.Claims.GetClaims;

public sealed class GetClaimsQueryHandler(IClaimsRepository repository) : IRequestHandler<GetClaimsQuery, IReadOnlyList<Claim>>
{
    public Task<IReadOnlyList<Claim>> Handle(GetClaimsQuery request, CancellationToken cancellationToken) => repository.GetClaimsAsync(cancellationToken);
}
