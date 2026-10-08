using MediatR;
using Claims.Domain;

namespace Claims.Application.Commands.Covers.DeleteCover;

public sealed record DeleteCoverCommand(int DisplayId) : IRequest<Cover?>;
