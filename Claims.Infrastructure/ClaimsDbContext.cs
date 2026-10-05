using Claims.Domain;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure;

public sealed class ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : DbContext(options)
{
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<Cover> Covers => Set<Cover>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Claim>(e => { e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedNever(); e.Property(x => x.Name).IsRequired(); e.Property(x => x.DamageCost).HasPrecision(18, 2); });
        modelBuilder.Entity<Cover>(e => { e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedNever(); e.Property(x => x.Premium).HasPrecision(18, 2); });
        modelBuilder.Entity<AuditEntry>(e => { e.HasKey(x => x.Id); e.Property(x => x.EntityId).IsRequired(); e.Property(x => x.Entity).IsRequired(); e.Property(x => x.Operation).IsRequired(); });
    }
}
