using System.Text.Json.Serialization;
using Claims.Application;
using Claims.Application.Queries.Claims.GetClaims;
using Claims.Domain;
using Claims.Infrastructure;
using Claims.Application.Validation;
using Claims.WebApi;
using FluentValidation;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddMediatR(options =>
{
    options.RegisterServicesFromAssembly(typeof(GetClaimsQuery).Assembly);
    options.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(GetClaimsQuery).Assembly);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<ClaimsDbContext>().Database.EnsureCreatedAsync();

app.Run();

public partial class Program { }
