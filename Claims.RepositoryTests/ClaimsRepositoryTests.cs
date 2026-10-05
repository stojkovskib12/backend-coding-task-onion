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
        var cover = new Cover("c1", new(2026, 1, 1), new(2026, 12, 31), CoverType.Yacht, 100m);
        await _repository.AddCoverAsync(cover, default);
        var claim = new Claim("q1", "c1", new(2026, 5, 5), "Damage", ClaimType.Fire, 42m);
        await _repository.AddClaimAsync(claim, default);
        Assert.Equal(claim, await _repository.GetClaimAsync("q1", default));
        Assert.Single(await _repository.GetClaimsAsync(default));
        Assert.Equal(cover, await _repository.GetCoverAsync("c1", default));
        await _repository.DeleteClaimAsync("q1", default);
        await _repository.DeleteCoverAsync("c1", default);
        Assert.Empty(await _repository.GetClaimsAsync(default));
        Assert.Null(await _repository.GetCoverAsync("c1", default));
    }
    public void Dispose() { _db.Dispose(); _connection.Dispose(); }
}
