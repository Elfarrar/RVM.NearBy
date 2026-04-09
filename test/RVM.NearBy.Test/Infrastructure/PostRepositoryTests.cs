using Microsoft.EntityFrameworkCore;
using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Domain.Enums;
using RVM.NearBy.Infrastructure.Data;
using RVM.NearBy.Infrastructure.Repositories;

namespace RVM.NearBy.Test.Infrastructure;

public class PostRepositoryTests
{
    private static NearByDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NearByDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NearByDbContext(options);
    }

    private static UserProfile CreateAuthor(NearByDbContext ctx)
    {
        var author = new UserProfile { Username = "author", DisplayName = "Author" };
        ctx.UserProfiles.Add(author);
        ctx.SaveChanges();
        return author;
    }

    [Fact]
    public async Task AddAsync_And_GetByIdAsync_Works()
    {
        using var ctx = CreateContext();
        var author = CreateAuthor(ctx);
        var repo = new PostRepository(ctx);

        var post = new Post { AuthorId = author.Id, Content = "Hello", Latitude = -23.55, Longitude = -46.63 };
        await repo.AddAsync(post);

        var found = await repo.GetByIdAsync(post.Id);
        Assert.NotNull(found);
        Assert.Equal("Hello", found.Content);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_IncludesAuthorAndMedia()
    {
        using var ctx = CreateContext();
        var author = CreateAuthor(ctx);
        var repo = new PostRepository(ctx);

        var post = new Post { AuthorId = author.Id, Content = "With media", Latitude = 0, Longitude = 0 };
        post.Media.Add(new PostMedia { PostId = post.Id, Url = "https://img.com/1.jpg", SortOrder = 0 });
        await repo.AddAsync(post);

        var found = await repo.GetByIdWithDetailsAsync(post.Id);
        Assert.NotNull(found);
        Assert.Equal("Author", found.Author.DisplayName);
        Assert.Single(found.Media);
    }

    [Fact]
    public async Task GetByAuthorAsync_FiltersByAuthor()
    {
        using var ctx = CreateContext();
        var author1 = CreateAuthor(ctx);
        var author2 = new UserProfile { Username = "other", DisplayName = "Other" };
        ctx.UserProfiles.Add(author2);
        ctx.SaveChanges();

        var repo = new PostRepository(ctx);
        await repo.AddAsync(new Post { AuthorId = author1.Id, Content = "A", Latitude = 0, Longitude = 0 });
        await repo.AddAsync(new Post { AuthorId = author1.Id, Content = "B", Latitude = 0, Longitude = 0 });
        await repo.AddAsync(new Post { AuthorId = author2.Id, Content = "C", Latitude = 0, Longitude = 0 });

        var posts = await repo.GetByAuthorAsync(author1.Id, 0, 10);
        Assert.Equal(2, posts.Count);
    }

    [Fact]
    public async Task GetNearbyAsync_FiltersPostsByDistance()
    {
        using var ctx = CreateContext();
        var author = CreateAuthor(ctx);
        var repo = new PostRepository(ctx);

        // Post in Sao Paulo center
        await repo.AddAsync(new Post { AuthorId = author.Id, Content = "SP Center", Latitude = -23.5505, Longitude = -46.6333 });
        // Post far away (Rio)
        await repo.AddAsync(new Post { AuthorId = author.Id, Content = "Rio", Latitude = -22.9068, Longitude = -43.1729 });

        var nearby = await repo.GetNearbyAsync(-23.55, -46.63, 5, 0, 10);
        Assert.Single(nearby);
        Assert.Equal("SP Center", nearby[0].Content);
    }

    [Fact]
    public async Task GetNearbyAsync_ExcludesPrivatePosts()
    {
        using var ctx = CreateContext();
        var author = CreateAuthor(ctx);
        var repo = new PostRepository(ctx);

        await repo.AddAsync(new Post { AuthorId = author.Id, Content = "Public", Latitude = -23.55, Longitude = -46.63, Visibility = PostVisibility.Public });
        await repo.AddAsync(new Post { AuthorId = author.Id, Content = "Private", Latitude = -23.55, Longitude = -46.63, Visibility = PostVisibility.Private });

        var nearby = await repo.GetNearbyAsync(-23.55, -46.63, 5, 0, 10);
        Assert.Single(nearby);
        Assert.Equal("Public", nearby[0].Content);
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsPublicPosts()
    {
        using var ctx = CreateContext();
        var author = CreateAuthor(ctx);
        var repo = new PostRepository(ctx);

        await repo.AddAsync(new Post { AuthorId = author.Id, Content = "Public", Latitude = 0, Longitude = 0, Visibility = PostVisibility.Public });
        await repo.AddAsync(new Post { AuthorId = author.Id, Content = "Private", Latitude = 0, Longitude = 0, Visibility = PostVisibility.Private });
        await repo.AddAsync(new Post { AuthorId = author.Id, Content = "NearbyOnly", Latitude = 0, Longitude = 0, Visibility = PostVisibility.NearbyOnly });

        var recent = await repo.GetRecentAsync(0, 10);
        Assert.Single(recent);
        Assert.Equal("Public", recent[0].Content);
    }

    [Fact]
    public async Task CountByAuthorAsync_Works()
    {
        using var ctx = CreateContext();
        var author = CreateAuthor(ctx);
        var repo = new PostRepository(ctx);

        await repo.AddAsync(new Post { AuthorId = author.Id, Content = "A", Latitude = 0, Longitude = 0 });
        await repo.AddAsync(new Post { AuthorId = author.Id, Content = "B", Latitude = 0, Longitude = 0 });

        var count = await repo.CountByAuthorAsync(author.Id);
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task DeleteAsync_RemovesPost()
    {
        using var ctx = CreateContext();
        var author = CreateAuthor(ctx);
        var repo = new PostRepository(ctx);

        var post = new Post { AuthorId = author.Id, Content = "Delete me", Latitude = 0, Longitude = 0 };
        await repo.AddAsync(post);
        await repo.DeleteAsync(post.Id);

        var found = await repo.GetByIdAsync(post.Id);
        Assert.Null(found);
    }

    [Fact]
    public async Task UpdateAsync_Works()
    {
        using var ctx = CreateContext();
        var author = CreateAuthor(ctx);
        var repo = new PostRepository(ctx);

        var post = new Post { AuthorId = author.Id, Content = "Original", Latitude = 0, Longitude = 0 };
        await repo.AddAsync(post);

        post.Content = "Updated";
        await repo.UpdateAsync(post);

        var found = await repo.GetByIdAsync(post.Id);
        Assert.Equal("Updated", found!.Content);
    }
}
