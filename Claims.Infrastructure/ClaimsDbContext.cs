using Claims.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Claims.Infrastructure;

public sealed class ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : DbContext(options)
{
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<Cover> Covers => Set<Cover>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var guidToString = new ValueConverter<Guid, string>(value => value.ToString(), value => Guid.Parse(value));
        modelBuilder.Entity<Claim>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasConversion(guidToString).HasMaxLength(36).ValueGeneratedNever();
            e.Property(x => x.CoverId).HasConversion(guidToString).HasMaxLength(36);
            e.Property(x => x.DisplayId).UseIdentityColumn();
            e.HasIndex(x => x.DisplayId).IsUnique();
            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.DamageCost).HasPrecision(18, 2);
            e.HasOne<Cover>().WithMany().HasForeignKey(x => x.CoverId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Cover>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasConversion(guidToString).HasMaxLength(36).ValueGeneratedNever();
            e.Property(x => x.DisplayId).UseIdentityColumn();
            e.HasIndex(x => x.DisplayId).IsUnique();
            e.Property(x => x.Premium).HasPrecision(18, 2);
        });
    }
}
