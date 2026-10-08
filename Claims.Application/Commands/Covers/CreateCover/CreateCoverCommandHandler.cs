using Claims.Application.Abstractions;
using Claims.Domain;
using MediatR;

namespace Claims.Application.Commands.Covers.CreateCover;

public sealed class CreateCoverCommandHandler(IClaimsRepository repository) : IRequestHandler<CreateCoverCommand, Cover>
{
    public async Task<Cover> Handle(CreateCoverCommand request, CancellationToken cancellationToken)
    {
        var draft = new Cover(Guid.NewGuid(), 0, request.StartDate, request.EndDate, request.Type, 0m);
        var cover = draft with { Premium = PremiumCalculator.Calculate(request.StartDate, request.EndDate, request.Type) };
        return await repository.AddCoverAsync(cover, cancellationToken);
    }
}
