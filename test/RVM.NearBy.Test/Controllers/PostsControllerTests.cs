using Microsoft.AspNetCore.Mvc;
using Moq;
using RVM.NearBy.API.Controllers;
using RVM.NearBy.API.Dtos;
using RVM.NearBy.API.Services;
using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Domain.Enums;
using RVM.NearBy.Domain.Interfaces;

namespace RVM.NearBy.Test.Controllers;

public class PostsControllerTests
{
    private readonly Mock<IPostRepository> _postRepo = new();
    private readonly Mock<ICommentRepository> _commentRepo = new();
    private readonly Mock<ILikeRepository> _likeRepo = new();
    // FeedService.MapToResponse is not virtual; use the real service with a stub repo
    private readonly FeedService _feedService;
    private readonly PostsController _controller;

    public PostsControllerTests()
    {
        var stubPostRepo = new Mock<IPostRepository>();
        _feedService = new FeedService(stubPostRepo.Object);

        _controller = new PostsController(
            _postRepo.Object,
            _commentRepo.Object,
            _likeRepo.Object,
            _feedService);
    }

    private static Post MakePost(string content = "Hello world")
        => new()
        {
            Content = content,
            Latitude = -23.5,
            Longitude = -46.6,
            Visibility = PostVisibility.Public,
            Author = new UserProfile { Username = "alice", DisplayName = "Alice" }
        };

    [Fact]
    public async Task GetById_ExistingPost_ReturnsOk()
    {
        var post = MakePost();
        _postRepo.Setup(r => r.GetByIdWithDetailsAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(post);

        var result = await _controller.GetById(post.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PostResponse>(ok.Value);
        Assert.Equal("Hello world", response.Content);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _postRepo.Setup(r => r.GetByIdWithDetailsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Post?)null);

        var result = await _controller.GetById(id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetByAuthor_ReturnsPosts()
    {
        var authorId = Guid.NewGuid();
        var posts = new List<Post> { MakePost("Post1"), MakePost("Post2") };
        _postRepo.Setup(r => r.GetByAuthorAsync(authorId, 0, 20, It.IsAny<CancellationToken>())).ReturnsAsync(posts);

        var result = await _controller.GetByAuthor(authorId, 0, 20, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<PostResponse>>(ok.Value);
        Assert.Equal(2, list.Count());
    }

    [Fact]
    public async Task Delete_ExistingPost_ReturnsNoContent()
    {
        var post = MakePost();
        _postRepo.Setup(r => r.GetByIdAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(post);
        _postRepo.Setup(r => r.DeleteAsync(post.Id, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _controller.Delete(post.Id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _postRepo.Verify(r => r.DeleteAsync(post.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_NotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _postRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Post?)null);

        var result = await _controller.Delete(id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetComments_ReturnsCommentList()
    {
        var postId = Guid.NewGuid();
        var author = new UserProfile { DisplayName = "Alice" };
        var comments = new List<Comment>
        {
            new() { PostId = postId, AuthorId = Guid.NewGuid(), Content = "Nice!", Author = author }
        };
        _commentRepo.Setup(r => r.GetByPostIdAsync(postId, 0, 20, It.IsAny<CancellationToken>())).ReturnsAsync(comments);

        var result = await _controller.GetComments(postId, 0, 20, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<CommentResponse>>(ok.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task AddComment_PostNotFound_Returns404()
    {
        var postId = Guid.NewGuid();
        _postRepo.Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>())).ReturnsAsync((Post?)null);

        var result = await _controller.AddComment(postId,
            new CreateCommentRequest(Guid.NewGuid(), "Hello"), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddComment_ExistingPost_AddsCommentAndReturnsCreated()
    {
        var post = MakePost();
        _postRepo.Setup(r => r.GetByIdAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(post);
        _commentRepo.Setup(r => r.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _commentRepo.Setup(r => r.CountByPostIdAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _postRepo.Setup(r => r.UpdateAsync(post, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var request = new CreateCommentRequest(Guid.NewGuid(), "Great post!");
        var result = await _controller.AddComment(post.Id, request, CancellationToken.None);

        Assert.IsType<CreatedResult>(result);
        _commentRepo.Verify(r => r.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteComment_ReturnsNoContent()
    {
        var postId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        _commentRepo.Setup(r => r.DeleteAsync(commentId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _postRepo.Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>())).ReturnsAsync(MakePost());
        _commentRepo.Setup(r => r.CountByPostIdAsync(postId, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _postRepo.Setup(r => r.UpdateAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _controller.DeleteComment(postId, commentId, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _commentRepo.Verify(r => r.DeleteAsync(commentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Like_PostNotFound_Returns404()
    {
        var postId = Guid.NewGuid();
        _postRepo.Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>())).ReturnsAsync((Post?)null);

        var result = await _controller.Like(postId, Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Like_AlreadyLiked_ReturnsConflict()
    {
        var post = MakePost();
        var userId = Guid.NewGuid();
        _postRepo.Setup(r => r.GetByIdAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(post);
        _likeRepo.Setup(r => r.GetByPostAndUserAsync(post.Id, userId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new Like { PostId = post.Id, UserId = userId });

        var result = await _controller.Like(post.Id, userId, CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Like_NewLike_ReturnsCreated()
    {
        var post = MakePost();
        var userId = Guid.NewGuid();
        _postRepo.Setup(r => r.GetByIdAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(post);
        _likeRepo.Setup(r => r.GetByPostAndUserAsync(post.Id, userId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Like?)null);
        _likeRepo.Setup(r => r.AddAsync(It.IsAny<Like>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _likeRepo.Setup(r => r.CountByPostIdAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _postRepo.Setup(r => r.UpdateAsync(post, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _controller.Like(post.Id, userId, CancellationToken.None);

        Assert.IsType<CreatedResult>(result);
        _likeRepo.Verify(r => r.AddAsync(It.IsAny<Like>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Unlike_NotFound_Returns404()
    {
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _likeRepo.Setup(r => r.GetByPostAndUserAsync(postId, userId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Like?)null);

        var result = await _controller.Unlike(postId, userId, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Unlike_ExistingLike_ReturnsNoContent()
    {
        var post = MakePost();
        var userId = Guid.NewGuid();
        var like = new Like { PostId = post.Id, UserId = userId };
        _likeRepo.Setup(r => r.GetByPostAndUserAsync(post.Id, userId, It.IsAny<CancellationToken>())).ReturnsAsync(like);
        _likeRepo.Setup(r => r.DeleteAsync(like.Id, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _postRepo.Setup(r => r.GetByIdAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(post);
        _likeRepo.Setup(r => r.CountByPostIdAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _postRepo.Setup(r => r.UpdateAsync(post, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _controller.Unlike(post.Id, userId, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _likeRepo.Verify(r => r.DeleteAsync(like.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
