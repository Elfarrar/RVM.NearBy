using Microsoft.EntityFrameworkCore;
using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Infrastructure.Data;
using RVM.NearBy.Infrastructure.Repositories;

namespace RVM.NearBy.Test.Infrastructure;

public class CommentAndLikeRepositoryTests
{
    private static NearByDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NearByDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NearByDbContext(options);
    }

    private static (UserProfile author, Post post) SeedAuthorAndPost(NearByDbContext ctx)
    {
        var author = new UserProfile { Username = "user", DisplayName = "User" };
        ctx.UserProfiles.Add(author);
        var post = new Post { AuthorId = author.Id, Content = "Post", Latitude = 0, Longitude = 0 };
        ctx.Posts.Add(post);
        ctx.SaveChanges();
        return (author, post);
    }

    // Comment tests
    [Fact]
    public async Task Comment_AddAndGetByPostId_Works()
    {
        using var ctx = CreateContext();
        var (author, post) = SeedAuthorAndPost(ctx);
        var repo = new CommentRepository(ctx);

        await repo.AddAsync(new Comment { PostId = post.Id, AuthorId = author.Id, Content = "Nice!" });
        await repo.AddAsync(new Comment { PostId = post.Id, AuthorId = author.Id, Content = "Cool!" });

        var comments = await repo.GetByPostIdAsync(post.Id, 0, 10);
        Assert.Equal(2, comments.Count);
    }

    [Fact]
    public async Task Comment_CountByPostId_Works()
    {
        using var ctx = CreateContext();
        var (author, post) = SeedAuthorAndPost(ctx);
        var repo = new CommentRepository(ctx);

        await repo.AddAsync(new Comment { PostId = post.Id, AuthorId = author.Id, Content = "A" });
        await repo.AddAsync(new Comment { PostId = post.Id, AuthorId = author.Id, Content = "B" });

        Assert.Equal(2, await repo.CountByPostIdAsync(post.Id));
    }

    [Fact]
    public async Task Comment_DeleteAsync_Works()
    {
        using var ctx = CreateContext();
        var (author, post) = SeedAuthorAndPost(ctx);
        var repo = new CommentRepository(ctx);

        var comment = new Comment { PostId = post.Id, AuthorId = author.Id, Content = "Delete me" };
        await repo.AddAsync(comment);
        await repo.DeleteAsync(comment.Id);

        Assert.Equal(0, await repo.CountByPostIdAsync(post.Id));
    }

    [Fact]
    public async Task Comment_Pagination_Works()
    {
        using var ctx = CreateContext();
        var (author, post) = SeedAuthorAndPost(ctx);
        var repo = new CommentRepository(ctx);

        for (var i = 0; i < 5; i++)
            await repo.AddAsync(new Comment { PostId = post.Id, AuthorId = author.Id, Content = $"Comment {i}" });

        var page = await repo.GetByPostIdAsync(post.Id, 2, 2);
        Assert.Equal(2, page.Count);
    }

    // Like tests
    [Fact]
    public async Task Like_AddAndGetByPostAndUser_Works()
    {
        using var ctx = CreateContext();
        var (author, post) = SeedAuthorAndPost(ctx);
        var repo = new LikeRepository(ctx);

        var like = new Like { PostId = post.Id, UserId = author.Id };
        await repo.AddAsync(like);

        var found = await repo.GetByPostAndUserAsync(post.Id, author.Id);
        Assert.NotNull(found);
    }

    [Fact]
    public async Task Like_GetByPostAndUser_NotFound_ReturnsNull()
    {
        using var ctx = CreateContext();
        var repo = new LikeRepository(ctx);
        Assert.Null(await repo.GetByPostAndUserAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task Like_CountByPostId_Works()
    {
        using var ctx = CreateContext();
        var (author, post) = SeedAuthorAndPost(ctx);
        var user2 = new UserProfile { Username = "user2", DisplayName = "User2" };
        ctx.UserProfiles.Add(user2);
        ctx.SaveChanges();

        var repo = new LikeRepository(ctx);
        await repo.AddAsync(new Like { PostId = post.Id, UserId = author.Id });
        await repo.AddAsync(new Like { PostId = post.Id, UserId = user2.Id });

        Assert.Equal(2, await repo.CountByPostIdAsync(post.Id));
    }

    [Fact]
    public async Task Like_DeleteAsync_Works()
    {
        using var ctx = CreateContext();
        var (author, post) = SeedAuthorAndPost(ctx);
        var repo = new LikeRepository(ctx);

        var like = new Like { PostId = post.Id, UserId = author.Id };
        await repo.AddAsync(like);
        await repo.DeleteAsync(like.Id);

        Assert.Equal(0, await repo.CountByPostIdAsync(post.Id));
    }
}
