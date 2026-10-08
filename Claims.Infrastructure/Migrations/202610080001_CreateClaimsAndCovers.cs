using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Claims.Infrastructure.Migrations;

[DbContext(typeof(ClaimsDbContext))]
[Migration("202610080001_CreateClaimsAndCovers")]
public sealed class CreateClaimsAndCovers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // This baseline supports both a new database and the prior EnsureCreated schema.
        migrationBuilder.Sql("""
            SET XACT_ABORT ON;

            IF OBJECT_ID(N'dbo.Covers', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Covers
                (
                    Id nvarchar(36) NOT NULL,
                    DisplayId int IDENTITY(1,1) NOT NULL,
                    StartDate date NOT NULL,
                    EndDate date NOT NULL,
                    Type int NOT NULL,
                    Premium decimal(18,2) NOT NULL,
                    CONSTRAINT PK_Covers PRIMARY KEY (Id)
                );
            END;

            IF OBJECT_ID(N'dbo.Claims', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Claims
                (
                    Id nvarchar(36) NOT NULL,
                    DisplayId int IDENTITY(1,1) NOT NULL,
                    CoverId nvarchar(36) NOT NULL,
                    Created date NOT NULL,
                    Name nvarchar(max) NOT NULL,
                    Type int NOT NULL,
                    DamageCost decimal(18,2) NOT NULL,
                    CONSTRAINT PK_Claims PRIMARY KEY (Id),
                    CONSTRAINT FK_Claims_Covers_CoverId FOREIGN KEY (CoverId) REFERENCES dbo.Covers (Id) ON DELETE NO ACTION
                );
            END;

            IF COL_LENGTH(N'dbo.Covers', N'DisplayId') IS NULL
                ALTER TABLE dbo.Covers ADD DisplayId int IDENTITY(1,1) NOT NULL;

            IF COL_LENGTH(N'dbo.Claims', N'DisplayId') IS NULL
                ALTER TABLE dbo.Claims ADD DisplayId int IDENTITY(1,1) NOT NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Covers_DisplayId' AND object_id = OBJECT_ID(N'dbo.Covers'))
                CREATE UNIQUE INDEX IX_Covers_DisplayId ON dbo.Covers(DisplayId);

            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Claims_DisplayId' AND object_id = OBJECT_ID(N'dbo.Claims'))
                CREATE UNIQUE INDEX IX_Claims_DisplayId ON dbo.Claims(DisplayId);

            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Claims_CoverId' AND object_id = OBJECT_ID(N'dbo.Claims'))
                CREATE INDEX IX_Claims_CoverId ON dbo.Claims(CoverId);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Claims', N'U') IS NOT NULL DROP TABLE dbo.Claims;
            IF OBJECT_ID(N'dbo.Covers', N'U') IS NOT NULL DROP TABLE dbo.Covers;
            """);
    }
}
