using Claims.Application.Abstractions;
using Claims.Domain;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure;

public sealed class ClaimsRepository(ClaimsDbContext db) : IClaimsRepository
{
    public async Task<IReadOnlyList<Claim>> GetClaimsAsync(CancellationToken ct) => await db.Claims.AsNoTracking().ToListAsync(ct);
    public Task<Claim?> GetClaimByDisplayIdAsync(int displayId, CancellationToken ct) => db.Claims.AsNoTracking().SingleOrDefaultAsync(x => x.DisplayId == displayId, ct);
    public async Task<Claim> AddClaimAsync(Claim claim, CancellationToken ct)
    {
        if (!db.Database.IsSqlServer())
            claim = claim with { DisplayId = (await db.Claims.MaxAsync(item => (int?)item.DisplayId, ct) ?? 0) + 1 };

        db.Claims.Add(claim);
        await db.SaveChangesAsync(ct);
        return claim;
    }
    public Task<bool> HasClaimsForCoverAsync(Guid coverId, CancellationToken ct) => db.Claims.AnyAsync(x => x.CoverId == coverId, ct);
    public async Task DeleteClaimByDisplayIdAsync(int displayId, CancellationToken ct) { var row = await db.Claims.SingleOrDefaultAsync(x => x.DisplayId == displayId, ct); if (row is not null) { db.Remove(row); await db.SaveChangesAsync(ct); } }
    public async Task<IReadOnlyList<Cover>> GetCoversAsync(CancellationToken ct) => await db.Covers.AsNoTracking().ToListAsync(ct);
    public Task<Cover?> GetCoverAsync(Guid id, CancellationToken ct) => db.Covers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<Cover?> GetCoverByDisplayIdAsync(int displayId, CancellationToken ct) => db.Covers.AsNoTracking().SingleOrDefaultAsync(x => x.DisplayId == displayId, ct);
    public async Task<Cover> AddCoverAsync(Cover cover, CancellationToken ct)
    {
        if (!db.Database.IsSqlServer())
            cover = cover with { DisplayId = (await db.Covers.MaxAsync(item => (int?)item.DisplayId, ct) ?? 0) + 1 };

        db.Covers.Add(cover);
        await db.SaveChangesAsync(ct);
        return cover;
    }
    public async Task DeleteCoverByDisplayIdAsync(int displayId, CancellationToken ct) { var row = await db.Covers.SingleOrDefaultAsync(x => x.DisplayId == displayId, ct); if (row is not null) { db.Remove(row); await db.SaveChangesAsync(ct); } }
}
