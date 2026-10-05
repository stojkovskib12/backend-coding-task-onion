using MediatR;

namespace Claims.Application.Commands.Claims.DeleteClaim;

public sealed record DeleteClaimCommand(string Id) : IRequest<bool>;
