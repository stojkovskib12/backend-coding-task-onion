using Claims.Domain;
using MediatR;

namespace Claims.Application.Queries.Claims.GetClaims;

public sealed record GetClaimsQuery : IRequest<IReadOnlyList<Claim>>;
