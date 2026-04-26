using Microsoft.AspNetCore.Mvc;
using Moq;
using RVM.NearBy.API.Controllers;
using RVM.NearBy.API.Dtos;
using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Domain.Interfaces;

namespace RVM.NearBy.Test.Controllers;

public class PlacesControllerTests
{
    private readonly Mock<IPlaceRepository> _placeRepo = new();
    private readonly PlacesController _controller;

    public PlacesControllerTests()
    {
        _controller = new PlacesController(_placeRepo.Object);
    }

    private static Place MakePlace(string name = "Cafe Paulista", string? category = "Food")
        => new()
        {
            Name = name,
            Description = "A nice place",
            Category = category,
            Latitude = -23.55,
            Longitude = -46.63,
            Address = "Rua X, 100"
        };

    [Fact]
    public async Task GetById_ExistingPlace_ReturnsOk()
    {
        var place = MakePlace();
        _placeRepo.Setup(r => r.GetByIdAsync(place.Id, It.IsAny<CancellationToken>())).ReturnsAsync(place);

        var result = await _controller.GetById(place.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PlaceResponse>(ok.Value);
        Assert.Equal(place.Id, response.Id);
        Assert.Equal("Cafe Paulista", response.Name);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _placeRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Place?)null);

        var result = await _controller.GetById(id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetNearby_ReturnsOkWithPlaces()
    {
        var places = new List<Place> { MakePlace("Place 1"), MakePlace("Place 2") };
        _placeRepo.Setup(r => r.GetNearbyAsync(-23.55, -46.63, 5, 20, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(places);

        var result = await _controller.GetNearby(-23.55, -46.63, 5, 20);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<PlaceResponse>>(ok.Value);
        Assert.Equal(2, list.Count());
    }

    [Fact]
    public async Task GetNearby_EmptyResult_ReturnsEmptyList()
    {
        _placeRepo.Setup(r => r.GetNearbyAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync([]);

        var result = await _controller.GetNearby(0, 0);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<PlaceResponse>>(ok.Value);
        Assert.Empty(list);
    }

    [Fact]
    public async Task Search_ReturnsMatchingPlaces()
    {
        var places = new List<Place> { MakePlace("Pizza Express") };
        _placeRepo.Setup(r => r.SearchAsync("pizza", 20, It.IsAny<CancellationToken>())).ReturnsAsync(places);

        var result = await _controller.Search("pizza");

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<PlaceResponse>>(ok.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task Search_NoMatches_ReturnsEmptyList()
    {
        _placeRepo.Setup(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync([]);

        var result = await _controller.Search("xyz");

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<PlaceResponse>>(ok.Value);
        Assert.Empty(list);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated()
    {
        var request = new CreatePlaceRequest("New Bar", "Great vibes", "Bar", -23.5, -46.6, "Av. Y, 200");
        _placeRepo.Setup(r => r.AddAsync(It.IsAny<Place>(), It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

        var result = await _controller.Create(request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<PlaceResponse>(created.Value);
        Assert.Equal("New Bar", response.Name);
        Assert.Equal("Bar", response.Category);
    }

    [Fact]
    public async Task Create_CallsAddAsync()
    {
        var request = new CreatePlaceRequest("Test Place", null, null, 0, 0, null);
        _placeRepo.Setup(r => r.AddAsync(It.IsAny<Place>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _controller.Create(request, CancellationToken.None);

        _placeRepo.Verify(r => r.AddAsync(It.Is<Place>(p => p.Name == "Test Place"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_NotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _placeRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Place?)null);

        var result = await _controller.Update(id, new UpdatePlaceRequest(null, null, null, null), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_ExistingPlace_UpdatesNameAndReturnsOk()
    {
        var place = MakePlace("Old Name");
        _placeRepo.Setup(r => r.GetByIdAsync(place.Id, It.IsAny<CancellationToken>())).ReturnsAsync(place);
        _placeRepo.Setup(r => r.UpdateAsync(place, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _controller.Update(place.Id, new UpdatePlaceRequest("New Name", null, null, null), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PlaceResponse>(ok.Value);
        Assert.Equal("New Name", response.Name);
        _placeRepo.Verify(r => r.UpdateAsync(place, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_NullFields_KeepsExistingValues()
    {
        var place = MakePlace("Original");
        _placeRepo.Setup(r => r.GetByIdAsync(place.Id, It.IsAny<CancellationToken>())).ReturnsAsync(place);
        _placeRepo.Setup(r => r.UpdateAsync(It.IsAny<Place>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _controller.Update(place.Id, new UpdatePlaceRequest(null, null, null, null), CancellationToken.None);

        Assert.Equal("Original", place.Name);
        Assert.Equal("Food", place.Category);
    }
}
