using Claims.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Claims.Infrastructure.Migrations;

[DbContext(typeof(ClaimsDbContext))]
public sealed class ClaimsDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "9.0.6")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity<Claim>(entity =>
        {
            entity.Property(claim => claim.Id).HasConversion<string>().HasMaxLength(36).ValueGeneratedNever().HasColumnType("nvarchar(36)");
            entity.Property(claim => claim.DisplayId).ValueGeneratedOnAdd().HasColumnType("int");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(entity.Property(claim => claim.DisplayId));
            entity.Property(claim => claim.CoverId).HasConversion<string>().HasMaxLength(36).IsRequired().HasColumnType("nvarchar(36)");
            entity.Property(claim => claim.Created).HasColumnType("date");
            entity.Property(claim => claim.Name).IsRequired().HasColumnType("nvarchar(max)");
            entity.Property(claim => claim.Type).HasColumnType("int");
            entity.Property(claim => claim.DamageCost).HasPrecision(18, 2).HasColumnType("decimal(18,2)");
            entity.HasKey(claim => claim.Id);
            entity.HasIndex(claim => claim.DisplayId).IsUnique();
            entity.HasIndex(claim => claim.CoverId);
            entity.HasOne<Cover>().WithMany().HasForeignKey(claim => claim.CoverId).OnDelete(DeleteBehavior.Restrict).IsRequired();
            entity.ToTable("Claims");
        });

        modelBuilder.Entity<Cover>(entity =>
        {
            entity.Property(cover => cover.Id).HasConversion<string>().HasMaxLength(36).ValueGeneratedNever().HasColumnType("nvarchar(36)");
            entity.Property(cover => cover.DisplayId).ValueGeneratedOnAdd().HasColumnType("int");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(entity.Property(cover => cover.DisplayId));
            entity.Property(cover => cover.StartDate).HasColumnType("date");
            entity.Property(cover => cover.EndDate).HasColumnType("date");
            entity.Property(cover => cover.Type).HasColumnType("int");
            entity.Property(cover => cover.Premium).HasPrecision(18, 2).HasColumnType("decimal(18,2)");
            entity.HasKey(cover => cover.Id);
            entity.HasIndex(cover => cover.DisplayId).IsUnique();
            entity.ToTable("Covers");
        });
#pragma warning restore 612, 618
    }
}
