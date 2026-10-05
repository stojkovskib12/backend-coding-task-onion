using Claims.Domain;
using MediatR;

namespace Claims.Application.Queries.Claims.GetClaim;

public sealed record GetClaimQuery(string Id) : IRequest<Claim?>;
