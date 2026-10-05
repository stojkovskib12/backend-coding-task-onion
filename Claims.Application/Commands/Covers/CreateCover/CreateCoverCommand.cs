using Claims.Domain;
using MediatR;

namespace Claims.Application.Commands.Covers.CreateCover;

public sealed record CreateCoverCommand(DateOnly StartDate, DateOnly EndDate, CoverType Type) : IRequest<Cover>;
