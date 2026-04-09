using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVM.NearBy.API.Dtos;
using RVM.NearBy.API.Services;
using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Domain.Interfaces;

namespace RVM.NearBy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PostsController(
    IPostRepository postRepo,
    ICommentRepository commentRepo,
    ILikeRepository likeRepo,
    FeedService feedService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var post = await postRepo.GetByIdWithDetailsAsync(id, ct);
        if (post is null) return NotFound();
        return Ok(feedService.MapToResponse(post));
    }

    [HttpGet("by-author/{authorId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByAuthor(Guid authorId, [FromQuery] int offset = 0,
        [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var posts = await postRepo.GetByAuthorAsync(authorId, offset, limit, ct);
        return Ok(posts.Select(feedService.MapToResponse));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePostRequest request, CancellationToken ct)
    {
        var post = await feedService.CreatePostAsync(request, ct);
        var full = await postRepo.GetByIdWithDetailsAsync(post.Id, ct);
        return CreatedAtAction(nameof(GetById), new { id = post.Id }, feedService.MapToResponse(full!));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var post = await postRepo.GetByIdAsync(id, ct);
        if (post is null) return NotFound();
        await postRepo.DeleteAsync(id, ct);
        return NoContent();
    }

    // Comments
    [HttpGet("{postId:guid}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> GetComments(Guid postId, [FromQuery] int offset = 0,
        [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var comments = await commentRepo.GetByPostIdAsync(postId, offset, limit, ct);
        return Ok(comments.Select(c => new CommentResponse(
            c.Id, c.PostId, c.AuthorId,
            c.Author?.DisplayName ?? "Unknown",
            c.Author?.AvatarUrl,
            c.Content, c.CreatedAt)));
    }

    [HttpPost("{postId:guid}/comments")]
    public async Task<IActionResult> AddComment(Guid postId, [FromBody] CreateCommentRequest request, CancellationToken ct)
    {
        var post = await postRepo.GetByIdAsync(postId, ct);
        if (post is null) return NotFound();

        var comment = new Comment
        {
            PostId = postId,
            AuthorId = request.AuthorId,
            Content = request.Content
        };

        await commentRepo.AddAsync(comment, ct);
        post.CommentCount = await commentRepo.CountByPostIdAsync(postId, ct);
        await postRepo.UpdateAsync(post, ct);

        return Created($"api/posts/{postId}/comments", new CommentResponse(
            comment.Id, comment.PostId, comment.AuthorId, "", null, comment.Content, comment.CreatedAt));
    }

    [HttpDelete("{postId:guid}/comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(Guid postId, Guid commentId, CancellationToken ct)
    {
        await commentRepo.DeleteAsync(commentId, ct);
        var post = await postRepo.GetByIdAsync(postId, ct);
        if (post is not null)
        {
            post.CommentCount = await commentRepo.CountByPostIdAsync(postId, ct);
            await postRepo.UpdateAsync(post, ct);
        }
        return NoContent();
    }

    // Likes
    [HttpPost("{postId:guid}/like")]
    public async Task<IActionResult> Like(Guid postId, [FromQuery] Guid userId, CancellationToken ct)
    {
        var post = await postRepo.GetByIdAsync(postId, ct);
        if (post is null) return NotFound();

        var existing = await likeRepo.GetByPostAndUserAsync(postId, userId, ct);
        if (existing is not null) return Conflict("Already liked");

        var like = new Like { PostId = postId, UserId = userId };
        await likeRepo.AddAsync(like, ct);

        post.LikeCount = await likeRepo.CountByPostIdAsync(postId, ct);
        await postRepo.UpdateAsync(post, ct);

        return Created($"api/posts/{postId}/like", new { postId, userId });
    }

    [HttpDelete("{postId:guid}/like")]
    public async Task<IActionResult> Unlike(Guid postId, [FromQuery] Guid userId, CancellationToken ct)
    {
        var existing = await likeRepo.GetByPostAndUserAsync(postId, userId, ct);
        if (existing is null) return NotFound();

        await likeRepo.DeleteAsync(existing.Id, ct);

        var post = await postRepo.GetByIdAsync(postId, ct);
        if (post is not null)
        {
            post.LikeCount = await likeRepo.CountByPostIdAsync(postId, ct);
            await postRepo.UpdateAsync(post, ct);
        }

        return NoContent();
    }
}
