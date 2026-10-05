using Claims.Application.Abstractions;
using Claims.Domain;
using MediatR;

namespace Claims.Application.Queries.Covers.GetCover;

public sealed class GetCoverQueryHandler(IClaimsRepository repository) : IRequestHandler<GetCoverQuery, Cover?>
{
    public Task<Cover?> Handle(GetCoverQuery request, CancellationToken cancellationToken) => repository.GetCoverAsync(request.Id, cancellationToken);
}
