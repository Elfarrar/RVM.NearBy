using Microsoft.AspNetCore.Mvc;
using Moq;
using RVM.NearBy.API.Controllers;
using RVM.NearBy.API.Dtos;
using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Domain.Interfaces;

namespace RVM.NearBy.Test.Controllers;

public class ProfilesControllerTests
{
    private readonly Mock<IUserProfileRepository> _profileRepo = new();
    private readonly Mock<IPostRepository> _postRepo = new();
    private readonly ProfilesController _controller;

    public ProfilesControllerTests()
    {
        _controller = new ProfilesController(_profileRepo.Object, _postRepo.Object);
    }

    private static UserProfile MakeProfile(string username = "alice", string display = "Alice")
        => new() { Username = username, DisplayName = display };

    [Fact]
    public async Task GetById_ExistingProfile_ReturnsOk()
    {
        var profile = MakeProfile();
        _profileRepo.Setup(r => r.GetByIdAsync(profile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _postRepo.Setup(r => r.CountByAuthorAsync(profile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(5);

        var result = await _controller.GetById(profile.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ProfileResponse>(ok.Value);
        Assert.Equal("alice", response.Username);
        Assert.Equal(5, response.PostCount);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _profileRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((UserProfile?)null);

        var result = await _controller.GetById(id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetByUsername_ExistingUser_ReturnsOk()
    {
        var profile = MakeProfile("bob", "Bob");
        _profileRepo.Setup(r => r.GetByUsernameAsync("bob", It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _postRepo.Setup(r => r.CountByAuthorAsync(profile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(3);

        var result = await _controller.GetByUsername("bob", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ProfileResponse>(ok.Value);
        Assert.Equal("bob", response.Username);
        Assert.Equal(3, response.PostCount);
    }

    [Fact]
    public async Task GetByUsername_NotFound_Returns404()
    {
        _profileRepo.Setup(r => r.GetByUsernameAsync("unknown", It.IsAny<CancellationToken>()))
                    .ReturnsAsync((UserProfile?)null);

        var result = await _controller.GetByUsername("unknown", CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Search_ReturnsProfilesWithPostCounts()
    {
        var profiles = new List<UserProfile> { MakeProfile("alice"), MakeProfile("alex") };
        _profileRepo.Setup(r => r.SearchAsync("al", 0, 20, It.IsAny<CancellationToken>())).ReturnsAsync(profiles);
        _postRepo.Setup(r => r.CountByAuthorAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var result = await _controller.Search("al", 0, 20, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<ProfileResponse>>(ok.Value);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task Search_EmptyResult_ReturnsEmptyList()
    {
        _profileRepo.Setup(r => r.SearchAsync(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync([]);

        var result = await _controller.Search(null, 0, 20, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<ProfileResponse>>(ok.Value);
        Assert.Empty(list);
    }

    [Fact]
    public async Task Create_UniqueUsername_ReturnsCreated()
    {
        _profileRepo.Setup(r => r.GetByUsernameAsync("newuser", It.IsAny<CancellationToken>()))
                    .ReturnsAsync((UserProfile?)null);
        _profileRepo.Setup(r => r.AddAsync(It.IsAny<UserProfile>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

        var request = new CreateProfileRequest("newuser", "New User", "Bio text", null);
        var result = await _controller.Create(request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<ProfileResponse>(created.Value);
        Assert.Equal("newuser", response.Username);
        Assert.Equal("New User", response.DisplayName);
    }

    [Fact]
    public async Task Create_DuplicateUsername_ReturnsConflict()
    {
        _profileRepo.Setup(r => r.GetByUsernameAsync("taken", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(MakeProfile("taken"));

        var request = new CreateProfileRequest("taken", "Another User", null, null);
        var result = await _controller.Create(request, CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Update_NotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _profileRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((UserProfile?)null);

        var result = await _controller.Update(id, new UpdateProfileRequest(null, null, null), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_ExistingProfile_UpdatesAndReturnsOk()
    {
        var profile = MakeProfile();
        _profileRepo.Setup(r => r.GetByIdAsync(profile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _profileRepo.Setup(r => r.UpdateAsync(profile, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _postRepo.Setup(r => r.CountByAuthorAsync(profile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(2);

        var result = await _controller.Update(profile.Id,
            new UpdateProfileRequest("New Display", "New bio", null), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ProfileResponse>(ok.Value);
        Assert.Equal("New Display", response.DisplayName);
        _profileRepo.Verify(r => r.UpdateAsync(profile, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateLocation_NotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _profileRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((UserProfile?)null);

        var result = await _controller.UpdateLocation(id, new UpdateLocationRequest(-23.5, -46.6), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateLocation_ExistingProfile_UpdatesCoordinates()
    {
        var profile = MakeProfile();
        _profileRepo.Setup(r => r.GetByIdAsync(profile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _profileRepo.Setup(r => r.UpdateAsync(profile, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _controller.UpdateLocation(profile.Id, new UpdateLocationRequest(-23.5, -46.6), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(-23.5, profile.LastLatitude);
        Assert.Equal(-46.6, profile.LastLongitude);
        Assert.NotNull(profile.LastLocationUpdate);
    }
}
