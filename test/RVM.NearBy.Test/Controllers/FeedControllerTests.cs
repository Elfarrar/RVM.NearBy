using Microsoft.AspNetCore.Mvc;
using Moq;
using RVM.NearBy.API.Controllers;
using RVM.NearBy.API.Dtos;
using RVM.NearBy.API.Services;
using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Domain.Enums;
using RVM.NearBy.Domain.Interfaces;

namespace RVM.NearBy.Test.Controllers;

public class FeedControllerTests
{
    private readonly Mock<IPostRepository> _postRepo = new();
    // FeedService.MapToResponse is not virtual; use real service with stub repo
    private readonly FeedService _feedService;
    private readonly FeedController _controller;

    public FeedControllerTests()
    {
        var stubPostRepo = new Mock<IPostRepository>();
        _feedService = new FeedService(stubPostRepo.Object);
        _controller = new FeedController(_postRepo.Object, _feedService);
    }

    private static Post MakePost(string content = "Sample post")
        => new()
        {
            Content = content,
            Latitude = -23.5,
            Longitude = -46.6,
            Visibility = PostVisibility.Public,
            Author = new UserProfile { Username = "user1", DisplayName = "User One" }
        };

    [Fact]
    public async Task GetNearby_ReturnsPosts()
    {
        var posts = new List<Post> { MakePost("Post A"), MakePost("Post B") };
        _postRepo.Setup(r => r.GetNearbyAsync(-23.5, -46.6, 5, 0, 20, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(posts);

        var result = await _controller.GetNearby(-23.5, -46.6, 5, 0, 20);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<PostResponse>>(ok.Value);
        Assert.Equal(2, list.Count());
    }

    [Fact]
    public async Task GetNearby_EmptyResult_ReturnsEmptyList()
    {
        _postRepo.Setup(r => r.GetNearbyAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(),
                                              It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync([]);

        var result = await _controller.GetNearby(0, 0);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<PostResponse>>(ok.Value);
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetNearby_CallsRepoWithCorrectParams()
    {
        _postRepo.Setup(r => r.GetNearbyAsync(-10.0, -35.0, 10, 5, 15, It.IsAny<CancellationToken>()))
                 .ReturnsAsync([]);

        await _controller.GetNearby(-10.0, -35.0, 10, 5, 15);

        _postRepo.Verify(r => r.GetNearbyAsync(-10.0, -35.0, 10, 5, 15, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRecent_ReturnsPosts()
    {
        var posts = new List<Post> { MakePost("Recent 1"), MakePost("Recent 2"), MakePost("Recent 3") };
        _postRepo.Setup(r => r.GetRecentAsync(0, 20, It.IsAny<CancellationToken>())).ReturnsAsync(posts);

        var result = await _controller.GetRecent(0, 20);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<PostResponse>>(ok.Value);
        Assert.Equal(3, list.Count());
    }

    [Fact]
    public async Task GetRecent_EmptyResult_ReturnsEmptyList()
    {
        _postRepo.Setup(r => r.GetRecentAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync([]);

        var result = await _controller.GetRecent();

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<PostResponse>>(ok.Value);
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetRecent_CallsRepoWithCorrectOffsetAndLimit()
    {
        _postRepo.Setup(r => r.GetRecentAsync(10, 5, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        await _controller.GetRecent(10, 5);

        _postRepo.Verify(r => r.GetRecentAsync(10, 5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetNearby_DefaultParams_UsesExpectedDefaults()
    {
        _postRepo.Setup(r => r.GetNearbyAsync(0, 0, 5, 0, 20, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        await _controller.GetNearby(0, 0);

        _postRepo.Verify(r => r.GetNearbyAsync(0, 0, 5, 0, 20, It.IsAny<CancellationToken>()), Times.Once);
    }
}
