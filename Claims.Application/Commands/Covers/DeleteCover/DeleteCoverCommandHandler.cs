using Claims.Application.Abstractions;
using Claims.Domain;
using MediatR;

namespace Claims.Application.Commands.Covers.DeleteCover;

public sealed class DeleteCoverCommandHandler(IClaimsRepository repository) : IRequestHandler<DeleteCoverCommand, Cover?>
{
    public async Task<Cover?> Handle(DeleteCoverCommand request, CancellationToken cancellationToken)
    {
        var cover = await repository.GetCoverByDisplayIdAsync(request.DisplayId, cancellationToken);
        if (cover is null) return null;
        await repository.DeleteCoverByDisplayIdAsync(request.DisplayId, cancellationToken);
        return cover;
    }
}
