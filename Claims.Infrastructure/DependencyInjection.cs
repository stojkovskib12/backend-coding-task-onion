using Claims.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Claims.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Claims")
            ?? "Server=localhost;Database=Claims;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
        services.AddDbContext<ClaimsDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IClaimsRepository, ClaimsRepository>();
        services.AddSingleton<AuditQueue>();
        services.AddSingleton<IAuditQueue>(sp => sp.GetRequiredService<AuditQueue>());
        services.AddHostedService(sp => sp.GetRequiredService<AuditQueue>());
        services.AddSingleton<ISystemClock, SystemClock>();
        return services;
    }
}
