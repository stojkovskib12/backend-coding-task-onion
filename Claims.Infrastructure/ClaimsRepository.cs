using Claims.Application.Abstractions;
using Claims.Domain;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure;

public sealed class ClaimsRepository(ClaimsDbContext db) : IClaimsRepository
{
    public async Task<IReadOnlyList<Claim>> GetClaimsAsync(CancellationToken ct) => await db.Claims.AsNoTracking().ToListAsync(ct);
    public Task<Claim?> GetClaimAsync(string id, CancellationToken ct) => db.Claims.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddClaimAsync(Claim claim, CancellationToken ct) { db.Claims.Add(claim); await db.SaveChangesAsync(ct); }
    public async Task DeleteClaimAsync(string id, CancellationToken ct) { var row = await db.Claims.FindAsync([id], ct); if (row is not null) { db.Remove(row); await db.SaveChangesAsync(ct); } }
    public async Task<IReadOnlyList<Cover>> GetCoversAsync(CancellationToken ct) => await db.Covers.AsNoTracking().ToListAsync(ct);
    public Task<Cover?> GetCoverAsync(string id, CancellationToken ct) => db.Covers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddCoverAsync(Cover cover, CancellationToken ct) { db.Covers.Add(cover); await db.SaveChangesAsync(ct); }
    public async Task DeleteCoverAsync(string id, CancellationToken ct) { var row = await db.Covers.FindAsync([id], ct); if (row is not null) { db.Remove(row); await db.SaveChangesAsync(ct); } }
}
