using MediatR;
using Claims.Domain;

namespace Claims.Application.Commands.Claims.DeleteClaim;

public sealed record DeleteClaimCommand(int DisplayId) : IRequest<Claim?>;
