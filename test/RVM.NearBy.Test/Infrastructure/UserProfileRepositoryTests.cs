using Microsoft.EntityFrameworkCore;
using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Infrastructure.Data;
using RVM.NearBy.Infrastructure.Repositories;

namespace RVM.NearBy.Test.Infrastructure;

public class UserProfileRepositoryTests
{
    private static NearByDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NearByDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NearByDbContext(options);
    }

    [Fact]
    public async Task AddAsync_And_GetByIdAsync_Works()
    {
        using var ctx = CreateContext();
        var repo = new UserProfileRepository(ctx);
        var profile = new UserProfile { Username = "alice", DisplayName = "Alice" };

        await repo.AddAsync(profile);
        var found = await repo.GetByIdAsync(profile.Id);

        Assert.NotNull(found);
        Assert.Equal("alice", found.Username);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        using var ctx = CreateContext();
        var repo = new UserProfileRepository(ctx);
        var found = await repo.GetByIdAsync(Guid.NewGuid());
        Assert.Null(found);
    }

    [Fact]
    public async Task GetByUsernameAsync_Works()
    {
        using var ctx = CreateContext();
        var repo = new UserProfileRepository(ctx);
        await repo.AddAsync(new UserProfile { Username = "bob", DisplayName = "Bob" });

        var found = await repo.GetByUsernameAsync("bob");
        Assert.NotNull(found);
        Assert.Equal("Bob", found.DisplayName);
    }

    [Fact]
    public async Task GetByUsernameAsync_NotFound_ReturnsNull()
    {
        using var ctx = CreateContext();
        var repo = new UserProfileRepository(ctx);
        var found = await repo.GetByUsernameAsync("nobody");
        Assert.Null(found);
    }

    [Fact]
    public async Task SearchAsync_FiltersByQuery()
    {
        using var ctx = CreateContext();
        var repo = new UserProfileRepository(ctx);
        await repo.AddAsync(new UserProfile { Username = "alice", DisplayName = "Alice Wonder" });
        await repo.AddAsync(new UserProfile { Username = "bob", DisplayName = "Bob Builder" });
        await repo.AddAsync(new UserProfile { Username = "charlie", DisplayName = "Charlie" });

        var results = await repo.SearchAsync("ali", 0, 10);
        Assert.Single(results);
        Assert.Equal("alice", results[0].Username);
    }

    [Fact]
    public async Task SearchAsync_NullQuery_ReturnsAll()
    {
        using var ctx = CreateContext();
        var repo = new UserProfileRepository(ctx);
        await repo.AddAsync(new UserProfile { Username = "alice", DisplayName = "Alice" });
        await repo.AddAsync(new UserProfile { Username = "bob", DisplayName = "Bob" });

        var results = await repo.SearchAsync(null, 0, 10);
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task SearchAsync_Pagination_Works()
    {
        using var ctx = CreateContext();
        var repo = new UserProfileRepository(ctx);
        await repo.AddAsync(new UserProfile { Username = "a", DisplayName = "A" });
        await repo.AddAsync(new UserProfile { Username = "b", DisplayName = "B" });
        await repo.AddAsync(new UserProfile { Username = "c", DisplayName = "C" });

        var page = await repo.SearchAsync(null, 1, 1);
        Assert.Single(page);
    }

    [Fact]
    public async Task UpdateAsync_Works()
    {
        using var ctx = CreateContext();
        var repo = new UserProfileRepository(ctx);
        var profile = new UserProfile { Username = "alice", DisplayName = "Alice" };
        await repo.AddAsync(profile);

        profile.DisplayName = "Alice Updated";
        await repo.UpdateAsync(profile);

        var found = await repo.GetByIdAsync(profile.Id);
        Assert.Equal("Alice Updated", found!.DisplayName);
    }
}
