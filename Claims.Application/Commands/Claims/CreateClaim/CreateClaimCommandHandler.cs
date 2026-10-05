using Claims.Application.Abstractions;
using Claims.Domain;
using MediatR;

namespace Claims.Application.Commands.Claims.CreateClaim;

public sealed class CreateClaimCommandHandler(IClaimsRepository repository, IAuditQueue audit) : IRequestHandler<CreateClaimCommand, Claim>
{
    public async Task<Claim> Handle(CreateClaimCommand request, CancellationToken cancellationToken)
    {
        var cover = await repository.GetCoverAsync(request.CoverId, cancellationToken);
        if (cover is null) throw new DomainValidationException("The related cover does not exist.");

        var claim = new Claim(Guid.NewGuid().ToString(), request.CoverId, request.Created, request.Name, request.Type, request.DamageCost);
        await repository.AddClaimAsync(claim, cancellationToken);
        audit.TryEnqueue(new("Claim", claim.Id, "POST", DateTimeOffset.UtcNow));
        return claim;
    }
}
