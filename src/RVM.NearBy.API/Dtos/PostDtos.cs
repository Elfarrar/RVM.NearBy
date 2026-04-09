namespace RVM.NearBy.API.Dtos;

public record CreatePostRequest(
    Guid AuthorId,
    string Content,
    double Latitude,
    double Longitude,
    string? LocationName,
    string? Visibility,
    Guid? PlaceId,
    List<CreateMediaRequest>? Media);

public record CreateMediaRequest(string Url, string? Type, int SortOrder, string? Caption);

public record PostResponse(
    Guid Id,
    Guid AuthorId,
    string AuthorName,
    string? AuthorAvatar,
    string Content,
    double Latitude,
    double Longitude,
    string? LocationName,
    string Visibility,
    Guid? PlaceId,
    string? PlaceName,
    int LikeCount,
    int CommentCount,
    List<MediaResponse>? Media,
    DateTime CreatedAt);

public record MediaResponse(Guid Id, string Url, string Type, int SortOrder, string? Caption);

public record FeedRequest(double Latitude, double Longitude, double? RadiusKm, int? Offset, int? Limit);
