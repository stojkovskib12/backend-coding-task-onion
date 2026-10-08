using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Claims.Domain;
using Claims.Infrastructure;
using Claims.Infrastructure.Auditing;
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
        Assert.NotEqual(Guid.Empty, cover.Id);
        Assert.True(cover.DisplayId > 0);
        Assert.True(cover.Premium > 0m, await coverResponse.Content.ReadAsStringAsync());
        Assert.Equal(PremiumCalculator.Calculate(start, start.AddDays(60), CoverType.Yacht), cover.Premium);

        var claimResponse = await _client.PostAsJsonAsync("/Claims", new { coverId = cover.Id, created = start.AddDays(1), name = "Hull damage", type = "Collision", damageCost = 900m });
        Assert.True(claimResponse.StatusCode == HttpStatusCode.Created, await claimResponse.Content.ReadAsStringAsync());
        var claim = await claimResponse.Content.ReadFromJsonAsync<Claim>(JsonOptions);
        Assert.NotNull(claim);
        Assert.NotEqual(Guid.Empty, claim.Id);
        Assert.True(claim.DisplayId > 0);
        Assert.Equal(cover.Id, claim.CoverId);
        Assert.Equal(claim.Id, (await _client.GetFromJsonAsync<Claim>($"/Claims/{claim.DisplayId}", JsonOptions))?.Id);

        var auditCount = 0;
        for (var attempt = 0; attempt < 30 && auditCount < 2; attempt++)
        {
            await Task.Delay(50);
            using var scope = _factory.Services.CreateScope();
            var audits = scope.ServiceProvider.GetRequiredService<AuditContext>();
            auditCount = await audits.ClaimAudits.CountAsync() + await audits.CoverAudits.CountAsync();
        }
        Assert.True(auditCount >= 3);
    }

    [Fact] public async Task Claim_and_cover_actions_are_audited_in_background()
    {
        var start = DateOnly.FromDateTime(DateTime.Today).AddDays(1);
        var end = start.AddDays(45);
        var coverResponse = await _client.PostAsJsonAsync("/Covers", new { startDate = start, endDate = end, type = "PassengerShip" });
        Assert.Equal(HttpStatusCode.Created, coverResponse.StatusCode);
        var cover = (await coverResponse.Content.ReadFromJsonAsync<Cover>(JsonOptions))!;

        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/Covers")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"/Covers/{cover.DisplayId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.PostAsync($"/Covers/compute?startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}&coverType=PassengerShip", null)).StatusCode);

        var claimResponse = await _client.PostAsJsonAsync("/Claims", new
        {
            coverId = cover.Id,
            created = start.AddDays(1),
            name = "Audit check",
            type = "Collision",
            damageCost = 250m
        });
        Assert.Equal(HttpStatusCode.Created, claimResponse.StatusCode);
        var claim = (await claimResponse.Content.ReadFromJsonAsync<Claim>(JsonOptions))!;

        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/Claims")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"/Claims/{claim.DisplayId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/Claims/{claim.DisplayId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/Covers/{cover.DisplayId}")).StatusCode);

        var claimAudited = false;
        var coverAudited = false;
        for (var attempt = 0; attempt < 40 && (!claimAudited || !coverAudited); attempt++)
        {
            await Task.Delay(50);
            using var scope = _factory.Services.CreateScope();
            var auditContext = scope.ServiceProvider.GetRequiredService<AuditContext>();
            claimAudited = await auditContext.ClaimAudits.CountAsync(audit => audit.ClaimId == claim.Id.ToString()) >= 3;
            coverAudited = await auditContext.CoverAudits.CountAsync(audit => audit.CoverId == cover.Id.ToString()) >= 3
                && await auditContext.CoverAudits.AnyAsync(audit => audit.CoverId == "*" && audit.HttpRequestType == "GET")
                && await auditContext.CoverAudits.AnyAsync(audit => audit.CoverId == "premium-calculation" && audit.HttpRequestType == "POST");
        }

        Assert.True(claimAudited, "Expected create, list/read and delete claim events to be persisted asynchronously.");
        Assert.True(coverAudited, "Expected cover lifecycle, collection read, and premium calculation events to be persisted asynchronously.");
    }

    [Fact] public async Task Invalid_claim_returns_bad_request()
    {
        var start = DateOnly.FromDateTime(DateTime.Today).AddDays(2);
        var coverResponse = await _client.PostAsJsonAsync("/Covers", new { startDate = start, endDate = start.AddDays(30), type = "Tanker" });
        var cover = await coverResponse.Content.ReadFromJsonAsync<Cover>(JsonOptions);
        var response = await _client.PostAsJsonAsync("/Claims", new { coverId = cover!.Id, created = start, name = "Too costly", type = "Fire", damageCost = 100001m });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact] public async Task Claim_outside_cover_period_returns_specific_validation_message()
    {
        var start = DateOnly.FromDateTime(DateTime.Today).AddDays(2);
        var end = start.AddDays(30);
        var coverResponse = await _client.PostAsJsonAsync("/Covers", new { startDate = start, endDate = end, type = "Yacht" });
        var cover = await coverResponse.Content.ReadFromJsonAsync<Cover>(JsonOptions);
        Assert.NotNull(cover);

        var response = await _client.PostAsJsonAsync("/Claims", new
        {
            coverId = cover.Id,
            created = end.AddDays(1),
            name = "Outside cover period",
            type = "Collision",
            damageCost = 50m
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Claim created date must fall within the cover period.", await response.Content.ReadAsStringAsync());
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
