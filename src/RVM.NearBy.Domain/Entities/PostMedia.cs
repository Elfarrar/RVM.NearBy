using RVM.NearBy.Domain.Enums;

namespace RVM.NearBy.Domain.Entities;

public class PostMedia
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid PostId { get; set; }
    public string Url { get; set; } = string.Empty;
    public MediaType Type { get; set; } = MediaType.Image;
    public int SortOrder { get; set; }
    public string? Caption { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Post Post { get; set; } = null!;
}
