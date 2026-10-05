using Claims.Application.Abstractions;
using MediatR;

namespace Claims.Application.Commands.Claims.DeleteClaim;

public sealed class DeleteClaimCommandHandler(IClaimsRepository repository, IAuditQueue audit) : IRequestHandler<DeleteClaimCommand, bool>
{
    public async Task<bool> Handle(DeleteClaimCommand request, CancellationToken cancellationToken)
    {
        if (await repository.GetClaimAsync(request.Id, cancellationToken) is null) return false;
        await repository.DeleteClaimAsync(request.Id, cancellationToken);
        audit.TryEnqueue(new("Claim", request.Id, "DELETE", DateTimeOffset.UtcNow));
        return true;
    }
}
