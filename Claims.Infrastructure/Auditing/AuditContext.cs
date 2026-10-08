using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Auditing;

/// <summary>Persists claim and cover audit records in their dedicated tables.</summary>
public sealed class AuditContext(DbContextOptions<AuditContext> options) : DbContext(options)
{
    public DbSet<ClaimAudit> ClaimAudits => Set<ClaimAudit>();
    public DbSet<CoverAudit> CoverAudits => Set<CoverAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClaimAudit>(entity =>
        {
            entity.ToTable("ClaimAudits");
            entity.HasKey(audit => audit.Id);
            entity.Property(audit => audit.ClaimId).IsRequired();
            entity.Property(audit => audit.Created).IsRequired();
            entity.Property(audit => audit.HttpRequestType).IsRequired();
        });

        modelBuilder.Entity<CoverAudit>(entity =>
        {
            entity.ToTable("CoverAudits");
            entity.HasKey(audit => audit.Id);
            entity.Property(audit => audit.CoverId).IsRequired();
            entity.Property(audit => audit.Created).IsRequired();
            entity.Property(audit => audit.HttpRequestType).IsRequired();
        });
    }
}
