using RVM.NearBy.Domain.Enums;

namespace RVM.NearBy.Domain.Entities;

public class Post
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? LocationName { get; set; }
    public PostVisibility Visibility { get; set; } = PostVisibility.Public;
    public Guid? PlaceId { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public UserProfile Author { get; set; } = null!;
    public Place? Place { get; set; }
    public ICollection<PostMedia> Media { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Like> Likes { get; set; } = [];
}
