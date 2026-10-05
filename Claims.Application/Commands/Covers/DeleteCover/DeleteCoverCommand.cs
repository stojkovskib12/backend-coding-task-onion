using MediatR;

namespace Claims.Application.Commands.Covers.DeleteCover;

public sealed record DeleteCoverCommand(string Id) : IRequest<bool>;
