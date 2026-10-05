using Claims.Domain;

namespace Claims.Application.Abstractions;

public interface IClaimsRepository
{
    Task<IReadOnlyList<Claim>> GetClaimsAsync(CancellationToken cancellationToken);
    Task<Claim?> GetClaimAsync(string id, CancellationToken cancellationToken);
    Task AddClaimAsync(Claim claim, CancellationToken cancellationToken);
    Task DeleteClaimAsync(string id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Cover>> GetCoversAsync(CancellationToken cancellationToken);
    Task<Cover?> GetCoverAsync(string id, CancellationToken cancellationToken);
    Task AddCoverAsync(Cover cover, CancellationToken cancellationToken);
    Task DeleteCoverAsync(string id, CancellationToken cancellationToken);
}
