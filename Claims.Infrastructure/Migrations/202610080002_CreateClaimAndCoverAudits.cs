using Claims.Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Claims.Infrastructure.Migrations;

[DbContext(typeof(AuditContext))]
[Migration("202610080002_CreateClaimAndCoverAudits")]
public sealed class CreateClaimAndCoverAudits : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ClaimAudits",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                ClaimId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                HttpRequestType = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ClaimAudits", row => row.Id));

        migrationBuilder.CreateTable(
            name: "CoverAudits",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                CoverId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                HttpRequestType = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_CoverAudits", row => row.Id));

        // Preserve events from the earlier unified audit table when upgrading an existing database.
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.AuditEntries', N'U') IS NOT NULL
            BEGIN
                INSERT INTO dbo.ClaimAudits (ClaimId, Created, HttpRequestType)
                SELECT EntityId, CAST(OccurredAt AS datetime2), Operation
                FROM dbo.AuditEntries
                WHERE Entity = N'Claim';

                INSERT INTO dbo.CoverAudits (CoverId, Created, HttpRequestType)
                SELECT EntityId, CAST(OccurredAt AS datetime2), Operation
                FROM dbo.AuditEntries
                WHERE Entity = N'Cover';

                DROP TABLE dbo.AuditEntries;
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ClaimAudits");
        migrationBuilder.DropTable(name: "CoverAudits");
    }
}
