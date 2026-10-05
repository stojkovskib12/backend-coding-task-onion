using Claims.Application.Abstractions;
using Claims.Domain;
using MediatR;

namespace Claims.Application.Commands.Covers.CreateCover;

public sealed class CreateCoverCommandHandler(IClaimsRepository repository, IAuditQueue audit) : IRequestHandler<CreateCoverCommand, Cover>
{
    public async Task<Cover> Handle(CreateCoverCommand request, CancellationToken cancellationToken)
    {
        var draft = new Cover(Guid.NewGuid().ToString(), request.StartDate, request.EndDate, request.Type, 0m);
        var cover = draft with { Premium = PremiumCalculator.Calculate(request.StartDate, request.EndDate, request.Type) };
        await repository.AddCoverAsync(cover, cancellationToken);
        audit.TryEnqueue(new("Cover", cover.Id, "POST", DateTimeOffset.UtcNow));
        return cover;
    }
}
