using Claims.Domain;
using MediatR;

namespace Claims.Application.Queries.Covers.GetCover;

public sealed record GetCoverQuery(int DisplayId) : IRequest<Cover?>;
