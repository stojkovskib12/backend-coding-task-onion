using Claims.Application.Abstractions;
using Claims.Domain;
using MediatR;

namespace Claims.Application.Commands.Claims.CreateClaim;

public sealed class CreateClaimCommandHandler(IClaimsRepository repository) : IRequestHandler<CreateClaimCommand, Claim>
{
    public async Task<Claim> Handle(CreateClaimCommand request, CancellationToken cancellationToken)
    {
        var cover = await repository.GetCoverAsync(request.CoverId, cancellationToken);
        if (cover is null) throw new DomainValidationException("The related cover does not exist.");

        var claim = new Claim(Guid.NewGuid(), 0, request.CoverId, request.Created, request.Name, request.Type, request.DamageCost);
        return await repository.AddClaimAsync(claim, cancellationToken);
    }
}
