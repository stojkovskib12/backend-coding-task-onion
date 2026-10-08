using Claims.Domain;

namespace Claims.Application.Abstractions;

public interface IClaimsRepository
{
    Task<IReadOnlyList<Claim>> GetClaimsAsync(CancellationToken cancellationToken);
    Task<Claim?> GetClaimByDisplayIdAsync(int displayId, CancellationToken cancellationToken);
    Task<Claim> AddClaimAsync(Claim claim, CancellationToken cancellationToken);
    Task<bool> HasClaimsForCoverAsync(Guid coverId, CancellationToken cancellationToken);
    Task DeleteClaimByDisplayIdAsync(int displayId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Cover>> GetCoversAsync(CancellationToken cancellationToken);
    Task<Cover?> GetCoverAsync(Guid id, CancellationToken cancellationToken);
    Task<Cover?> GetCoverByDisplayIdAsync(int displayId, CancellationToken cancellationToken);
    Task<Cover> AddCoverAsync(Cover cover, CancellationToken cancellationToken);
    Task DeleteCoverByDisplayIdAsync(int displayId, CancellationToken cancellationToken);
}
