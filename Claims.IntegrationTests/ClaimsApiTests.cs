using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Claims.Domain;
using Claims.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Claims.IntegrationTests;

public sealed class ClaimsApiTests : IClassFixture<ClaimsApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() } };
    private readonly HttpClient _client;
    private readonly ClaimsApiFactory _factory;
    public ClaimsApiTests(ClaimsApiFactory factory) { _factory = factory; _client = factory.CreateClient(); }

    [Fact] public async Task Cover_and_claim_are_created_and_read_over_http()
    {
        var start = DateOnly.FromDateTime(DateTime.Today).AddDays(1);
        var coverResponse = await _client.PostAsJsonAsync("/Covers", new { startDate = start, endDate = start.AddDays(60), type = "Yacht" });
        Assert.Equal(HttpStatusCode.Created, coverResponse.StatusCode);
        var cover = await coverResponse.Content.ReadFromJsonAsync<Cover>(JsonOptions);
        Assert.NotNull(cover);
        Assert.True(cover.Premium > 0m, await coverResponse.Content.ReadAsStringAsync());
        Assert.Equal(PremiumCalculator.Calculate(start, start.AddDays(60), CoverType.Yacht), cover.Premium);

        var claimResponse = await _client.PostAsJsonAsync("/Claims", new { coverId = cover.Id, created = start.AddDays(1), name = "Hull damage", type = "Collision", damageCost = 900m });
        Assert.True(claimResponse.StatusCode == HttpStatusCode.Created, await claimResponse.Content.ReadAsStringAsync());
        var claim = await claimResponse.Content.ReadFromJsonAsync<Claim>(JsonOptions);
        Assert.NotNull(claim);
        Assert.Equal(claim.Id, (await _client.GetFromJsonAsync<Claim>($"/Claims/{claim.Id}", JsonOptions))?.Id);

        var auditCount = 0;
        for (var attempt = 0; attempt < 30 && auditCount < 2; attempt++)
        {
            await Task.Delay(50);
            using var scope = _factory.Services.CreateScope();
            auditCount = await scope.ServiceProvider.GetRequiredService<ClaimsDbContext>().AuditEntries.CountAsync();
        }
        Assert.True(auditCount >= 2);
    }

    [Fact] public async Task Invalid_claim_returns_bad_request()
    {
        var start = DateOnly.FromDateTime(DateTime.Today).AddDays(2);
        var coverResponse = await _client.PostAsJsonAsync("/Covers", new { startDate = start, endDate = start.AddDays(30), type = "Tanker" });
        var cover = await coverResponse.Content.ReadFromJsonAsync<Cover>(JsonOptions);
        var response = await _client.PostAsJsonAsync("/Claims", new { coverId = cover!.Id, created = start, name = "Too costly", type = "Fire", damageCost = 100001m });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact] public async Task Invalid_cover_period_returns_validation_problem()
    {
        var start = DateOnly.FromDateTime(DateTime.Today).AddDays(1);
        var response = await _client.PostAsJsonAsync("/Covers", new { startDate = start, endDate = start.AddYears(1).AddDays(1), type = "Yacht" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("errors", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact] public async Task Swagger_ui_is_available_at_default_path()
    {
        var response = await _client.GetAsync("/swagger/index.html");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Swagger UI", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }
}
