using Microsoft.EntityFrameworkCore;
using RVM.NearBy.API.Dtos;
using RVM.NearBy.API.Services;
using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Domain.Enums;
using RVM.NearBy.Infrastructure.Data;
using RVM.NearBy.Infrastructure.Repositories;

#pragma warning disable CS8602
namespace RVM.NearBy.Test.Services;

public class FeedServiceTests
{
    private static (NearByDbContext ctx, FeedService svc) CreateService()
    {
        var options = new DbContextOptionsBuilder<NearByDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new NearByDbContext(options);
        var postRepo = new PostRepository(ctx);
        return (ctx, new FeedService(postRepo));
    }

    [Fact]
    public async Task CreatePostAsync_CreatesPost()
    {
        var (ctx, svc) = CreateService();
        var author = new UserProfile { Username = "alice", DisplayName = "Alice" };
        ctx.UserProfiles.Add(author);
        ctx.SaveChanges();

        var request = new CreatePostRequest(author.Id, "Hello!", -23.55, -46.63, "SP", null, null, null);
        var post = await svc.CreatePostAsync(request, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, post.Id);
        Assert.Equal("Hello!", post.Content);
        Assert.Equal(PostVisibility.Public, post.Visibility);
    }

    [Fact]
    public async Task CreatePostAsync_WithMedia_AttachesMedia()
    {
        var (ctx, svc) = CreateService();
        var author = new UserProfile { Username = "bob", DisplayName = "Bob" };
        ctx.UserProfiles.Add(author);
        ctx.SaveChanges();

        var media = new List<CreateMediaRequest>
        {
            new("https://img.com/1.jpg", "Image", 0, "Photo 1"),
            new("https://img.com/2.jpg", "Image", 1, null)
        };

        var request = new CreatePostRequest(author.Id, "Photos!", 0, 0, null, null, null, media);
        var post = await svc.CreatePostAsync(request, CancellationToken.None);

        Assert.Equal(2, post.Media.Count);
    }

    [Fact]
    public async Task CreatePostAsync_WithVisibility_ParsesCorrectly()
    {
        var (ctx, svc) = CreateService();
        var author = new UserProfile { Username = "charlie", DisplayName = "Charlie" };
        ctx.UserProfiles.Add(author);
        ctx.SaveChanges();

        var request = new CreatePostRequest(author.Id, "Nearby only", 0, 0, null, "NearbyOnly", null, null);
        var post = await svc.CreatePostAsync(request, CancellationToken.None);

        Assert.Equal(PostVisibility.NearbyOnly, post.Visibility);
    }

    [Fact]
    public async Task CreatePostAsync_InvalidVisibility_DefaultsToPublic()
    {
        var (ctx, svc) = CreateService();
        var author = new UserProfile { Username = "dave", DisplayName = "Dave" };
        ctx.UserProfiles.Add(author);
        ctx.SaveChanges();

        var request = new CreatePostRequest(author.Id, "Bad vis", 0, 0, null, "invalid", null, null);
        var post = await svc.CreatePostAsync(request, CancellationToken.None);

        Assert.Equal(PostVisibility.Public, post.Visibility);
    }

    [Fact]
    public void MapToResponse_MapsCorrectly()
    {
        var (_, svc) = CreateService();
        var author = new UserProfile { Username = "eve", DisplayName = "Eve", AvatarUrl = "https://avatar.com/eve.png" };
        var place = new Place { Name = "Park" };
        var post = new Post
        {
            AuthorId = author.Id,
            Author = author,
            Content = "At the park",
            Latitude = -23.55,
            Longitude = -46.63,
            LocationName = "Ibirapuera",
            Visibility = PostVisibility.Public,
            PlaceId = place.Id,
            Place = place,
            LikeCount = 5,
            CommentCount = 2
        };
        post.Media.Add(new PostMedia { PostId = post.Id, Url = "https://img.com/1.jpg", Type = MediaType.Image, SortOrder = 0 });

        var response = svc.MapToResponse(post);

        Assert.Equal("Eve", response.AuthorName);
        Assert.Equal("https://avatar.com/eve.png", response.AuthorAvatar);
        Assert.Equal("At the park", response.Content);
        Assert.Equal("Ibirapuera", response.LocationName);
        Assert.Equal("Park", response.PlaceName);
        Assert.Equal(5, response.LikeCount);
        Assert.Equal(2, response.CommentCount);
        Assert.Single(response.Media!);
    }
}
