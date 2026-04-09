namespace RVM.NearBy.API.Dtos;

public record CreateCommentRequest(Guid AuthorId, string Content);

public record CommentResponse(
    Guid Id,
    Guid PostId,
    Guid AuthorId,
    string AuthorName,
    string? AuthorAvatar,
    string Content,
    DateTime CreatedAt);
