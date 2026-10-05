using Claims.Application;
using Claims.Application.Abstractions;
using Claims.Application.Validation;
using Claims.Application.Commands.Claims.CreateClaim;
using Claims.Application.Commands.Covers.CreateCover;
using Claims.Domain;
using Moq;
using Xunit;

namespace Claims.UnitTests;

public sealed class FluentValidationTests
{
    private static readonly DateOnly Today = new(2026, 1, 1);

    [Theory]
    [InlineData(100_000, true)]
    [InlineData(100_000.01, false)]
    [InlineData(-1, false)]
    public async Task Claim_damage_cost_is_validated(decimal cost, bool valid)
    {
        var repo = new Mock<IClaimsRepository>();
        repo.Setup(x => x.GetCoverAsync("c1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Cover("c1", Today, Today.AddYears(1), CoverType.Yacht, 1m));
        var validator = new CreateClaimCommandValidator(repo.Object);
        var result = await validator.ValidateAsync(new CreateClaimCommand("c1", Today, "Test", ClaimType.Fire, cost));
        Assert.Equal(valid, result.IsValid);
    }

    [Theory]
    [InlineData(2025, 12, 31, false)]
    [InlineData(2026, 1, 1, true)]
    [InlineData(2026, 12, 31, true)]
    [InlineData(2027, 1, 2, false)]
    public async Task Claim_date_must_be_inside_related_cover(int year, int month, int day, bool valid)
    {
        var repo = new Mock<IClaimsRepository>();
        repo.Setup(x => x.GetCoverAsync("c1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Cover("c1", Today, Today.AddYears(1), CoverType.Yacht, 1m));
        var result = await new CreateClaimCommandValidator(repo.Object)
            .ValidateAsync(new CreateClaimCommand("c1", new DateOnly(year, month, day), "Test", ClaimType.Fire, 1m));
        Assert.Equal(valid, result.IsValid);
    }

    [Fact] public async Task Claim_requires_related_cover()
    {
        var repo = new Mock<IClaimsRepository>();
        repo.Setup(x => x.GetCoverAsync("missing", It.IsAny<CancellationToken>())).ReturnsAsync((Cover?)null);
        var result = await new CreateClaimCommandValidator(repo.Object)
            .ValidateAsync(new CreateClaimCommand("missing", Today, "Test", ClaimType.Fire, 1m));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateClaimCommand.CoverId));
    }

    [Fact] public async Task Cover_rejects_past_start_and_period_longer_than_one_year()
    {
        var clock = new Mock<ISystemClock>();
        clock.SetupGet(x => x.Today).Returns(Today);
        var validator = new CreateCoverCommandValidator(clock.Object);
        var past = await validator.ValidateAsync(new CreateCoverCommand(Today.AddDays(-1), Today.AddDays(10), CoverType.Yacht));
        var longPeriod = await validator.ValidateAsync(new CreateCoverCommand(Today, Today.AddYears(1).AddDays(1), CoverType.Yacht));
        Assert.Contains(past.Errors, error => error.PropertyName == nameof(CreateCoverCommand.StartDate));
        Assert.Contains(longPeriod.Errors, error => error.PropertyName == nameof(CreateCoverCommand.EndDate));
    }
}
