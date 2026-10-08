using Claims.Infrastructure;
using Claims.Infrastructure.Auditing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Claims.IntegrationTests;

public sealed class ClaimsApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new($"Data Source=claims-tests-{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
    private readonly SqliteConnection _auditConnection = new($"Data Source=claims-audit-tests-{Guid.NewGuid():N};Mode=Memory;Cache=Shared");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        _auditConnection.Open();
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ClaimsDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ClaimsDbContext>>();
            services.RemoveAll<ClaimsDbContext>();
            services.AddSingleton(_connection);
            services.AddDbContext<ClaimsDbContext>(options => options.UseSqlite(_connection.ConnectionString));
            services.RemoveAll<DbContextOptions<AuditContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AuditContext>>();
            services.RemoveAll<AuditContext>();
            services.AddSingleton(_auditConnection);
            services.AddDbContext<AuditContext>(options => options.UseSqlite(_auditConnection.ConnectionString));
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
            _auditConnection.Dispose();
        }

        base.Dispose(disposing);
    }
}
