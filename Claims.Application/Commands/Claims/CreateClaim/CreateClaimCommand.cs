using Claims.Domain;
using MediatR;

namespace Claims.Application.Commands.Claims.CreateClaim;

public sealed record CreateClaimCommand(Guid CoverId, DateOnly Created, string Name, ClaimType Type, decimal DamageCost) : IRequest<Claim>;
