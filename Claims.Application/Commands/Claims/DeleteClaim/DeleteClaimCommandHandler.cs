using Claims.Application.Abstractions;
using Claims.Domain;
using MediatR;

namespace Claims.Application.Commands.Claims.DeleteClaim;

public sealed class DeleteClaimCommandHandler(IClaimsRepository repository) : IRequestHandler<DeleteClaimCommand, Claim?>
{
    public async Task<Claim?> Handle(DeleteClaimCommand request, CancellationToken cancellationToken)
    {
        var claim = await repository.GetClaimByDisplayIdAsync(request.DisplayId, cancellationToken);
        if (claim is null) return null;
        await repository.DeleteClaimByDisplayIdAsync(request.DisplayId, cancellationToken);
        return claim;
    }
}
