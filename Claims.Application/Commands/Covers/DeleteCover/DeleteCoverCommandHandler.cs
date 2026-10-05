using Claims.Application.Abstractions;
using MediatR;

namespace Claims.Application.Commands.Covers.DeleteCover;

public sealed class DeleteCoverCommandHandler(IClaimsRepository repository, IAuditQueue audit) : IRequestHandler<DeleteCoverCommand, bool>
{
    public async Task<bool> Handle(DeleteCoverCommand request, CancellationToken cancellationToken)
    {
        if (await repository.GetCoverAsync(request.Id, cancellationToken) is null) return false;
        await repository.DeleteCoverAsync(request.Id, cancellationToken);
        audit.TryEnqueue(new("Cover", request.Id, "DELETE", DateTimeOffset.UtcNow));
        return true;
    }
}
