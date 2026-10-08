using Claims.Domain;
using Claims.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Claims.RepositoryTests;

public sealed class ClaimsRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly ClaimsDbContext _db;
    private readonly ClaimsRepository _repository;
    public ClaimsRepositoryTests()
    {
        _connection.Open();
        _db = new ClaimsDbContext(new DbContextOptionsBuilder<ClaimsDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
        _repository = new ClaimsRepository(_db);
    }
    [Fact] public async Task Claim_and_cover_crud_round_trip()
    {
        var cover = new Cover(Guid.NewGuid(), 0, new(2026, 1, 1), new(2026, 12, 31), CoverType.Yacht, 100m);
        cover = await _repository.AddCoverAsync(cover, default);
        var claim = new Claim(Guid.NewGuid(), 0, cover.Id, new(2026, 5, 5), "Damage", ClaimType.Fire, 42m);
        claim = await _repository.AddClaimAsync(claim, default);
        Assert.NotEqual(Guid.Empty, cover.Id);
        Assert.NotEqual(Guid.Empty, claim.Id);
        Assert.True(cover.DisplayId > 0);
        Assert.True(claim.DisplayId > 0);
        Assert.Equal(claim, await _repository.GetClaimByDisplayIdAsync(claim.DisplayId, default));
        Assert.Single(await _repository.GetClaimsAsync(default));
        Assert.Equal(cover, await _repository.GetCoverAsync(cover.Id, default));
        await _repository.DeleteClaimByDisplayIdAsync(claim.DisplayId, default);
        await _repository.DeleteCoverByDisplayIdAsync(cover.DisplayId, default);
        Assert.Empty(await _repository.GetClaimsAsync(default));
        Assert.Null(await _repository.GetCoverAsync(cover.Id, default));
    }
    public void Dispose() { _db.Dispose(); _connection.Dispose(); }
}
