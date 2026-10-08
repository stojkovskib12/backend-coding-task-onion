using Claims.Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Claims.Infrastructure.Migrations;

[DbContext(typeof(AuditContext))]
public sealed class AuditContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "9.0.6")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("Claims.Infrastructure.Auditing.ClaimAudit", entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(entity.Property<int>("Id"));
            entity.Property<string>("ClaimId").IsRequired().HasColumnType("nvarchar(max)");
            entity.Property<DateTime>("Created").HasColumnType("datetime2");
            entity.Property<string>("HttpRequestType").IsRequired().HasColumnType("nvarchar(max)");
            entity.HasKey("Id");
            entity.ToTable("ClaimAudits");
        });

        modelBuilder.Entity("Claims.Infrastructure.Auditing.CoverAudit", entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(entity.Property<int>("Id"));
            entity.Property<string>("CoverId").IsRequired().HasColumnType("nvarchar(max)");
            entity.Property<DateTime>("Created").HasColumnType("datetime2");
            entity.Property<string>("HttpRequestType").IsRequired().HasColumnType("nvarchar(max)");
            entity.HasKey("Id");
            entity.ToTable("CoverAudits");
        });
#pragma warning restore 612, 618
    }
}
