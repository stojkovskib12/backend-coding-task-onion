using Claims.Domain;
using MediatR;

namespace Claims.Application.Queries.Covers.GetCovers;

public sealed record GetCoversQuery : IRequest<IReadOnlyList<Cover>>;
