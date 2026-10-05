using Claims.Application.Abstractions;
using Claims.Domain;
using MediatR;

namespace Claims.Application.Queries.Covers.GetCovers;

public sealed class GetCoversQueryHandler(IClaimsRepository repository) : IRequestHandler<GetCoversQuery, IReadOnlyList<Cover>>
{
    public Task<IReadOnlyList<Cover>> Handle(GetCoversQuery request, CancellationToken cancellationToken) => repository.GetCoversAsync(cancellationToken);
}
