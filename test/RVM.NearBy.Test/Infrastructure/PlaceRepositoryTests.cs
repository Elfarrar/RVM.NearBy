using Microsoft.EntityFrameworkCore;
using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Infrastructure.Data;
using RVM.NearBy.Infrastructure.Repositories;

namespace RVM.NearBy.Test.Infrastructure;

public class PlaceRepositoryTests
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
        var repo = new PlaceRepository(ctx);
        var place = new Place { Name = "Park", Latitude = -23.55, Longitude = -46.63 };

        await repo.AddAsync(place);
        var found = await repo.GetByIdAsync(place.Id);

        Assert.NotNull(found);
        Assert.Equal("Park", found.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        using var ctx = CreateContext();
        var repo = new PlaceRepository(ctx);
        Assert.Null(await repo.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetNearbyAsync_FiltersAndSortsByDistance()
    {
        using var ctx = CreateContext();
        var repo = new PlaceRepository(ctx);

        await repo.AddAsync(new Place { Name = "Close", Latitude = -23.551, Longitude = -46.631 });
        await repo.AddAsync(new Place { Name = "Far", Latitude = -22.90, Longitude = -43.17 });

        var nearby = await repo.GetNearbyAsync(-23.55, -46.63, 5, 10);
        Assert.Single(nearby);
        Assert.Equal("Close", nearby[0].Name);
    }

    [Fact]
    public async Task SearchAsync_FiltersByNameOrCategory()
    {
        using var ctx = CreateContext();
        var repo = new PlaceRepository(ctx);

        await repo.AddAsync(new Place { Name = "Coffee Shop", Category = "Cafe", Latitude = 0, Longitude = 0 });
        await repo.AddAsync(new Place { Name = "Gym", Category = "Fitness", Latitude = 0, Longitude = 0 });

        var byName = await repo.SearchAsync("Coffee", 10);
        Assert.Single(byName);

        var byCat = await repo.SearchAsync("Fitness", 10);
        Assert.Single(byCat);
    }

    [Fact]
    public async Task UpdateAsync_Works()
    {
        using var ctx = CreateContext();
        var repo = new PlaceRepository(ctx);
        var place = new Place { Name = "Old Name", Latitude = 0, Longitude = 0 };
        await repo.AddAsync(place);

        place.Name = "New Name";
        await repo.UpdateAsync(place);

        var found = await repo.GetByIdAsync(place.Id);
        Assert.Equal("New Name", found!.Name);
    }
}
