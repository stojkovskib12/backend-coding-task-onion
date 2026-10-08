using System.Text.Json.Serialization;
using Claims.Application;
using Claims.Application.Queries.Claims.GetClaims;
using Claims.Domain;
using Claims.Infrastructure;
using Claims.Application.Validation;
using Claims.Application.Auditing;
using Claims.Infrastructure.Auditing;
using Claims.WebApi;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddCors(options => options.AddPolicy("ClaimsClient", policy =>
    policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:5054", "http://127.0.0.1:5054")
        .AllowAnyHeader()
        .AllowAnyMethod()));
builder.Services.AddMediatR(options =>
{
    options.RegisterServicesFromAssembly(typeof(GetClaimsQuery).Assembly);
    options.AddOpenBehavior(typeof(ValidationBehavior<,>));
    options.AddOpenBehavior(typeof(AuditingBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(GetClaimsQuery).Assembly);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
    var domainXmlFile = $"{typeof(Claim).Assembly.GetName().Name}.xml";
    var domainXmlPath = Path.Combine(AppContext.BaseDirectory, domainXmlFile);
    if (File.Exists(domainXmlPath))
        options.IncludeXmlComments(domainXmlPath);
});

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("ClaimsClient");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var claimsContext = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
    var auditContext = scope.ServiceProvider.GetRequiredService<AuditContext>();
    if (claimsContext.Database.IsSqlServer())
    {
        await claimsContext.Database.MigrateAsync();
        await auditContext.Database.MigrateAsync();
    }
    else
    {
        await claimsContext.Database.EnsureCreatedAsync();
        await auditContext.Database.EnsureCreatedAsync();
    }
}

app.Run();

public partial class Program { }
