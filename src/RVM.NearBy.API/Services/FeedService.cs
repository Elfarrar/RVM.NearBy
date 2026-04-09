using RVM.NearBy.API.Dtos;
using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Domain.Enums;
using RVM.NearBy.Domain.Interfaces;

namespace RVM.NearBy.API.Services;

public class FeedService(IPostRepository postRepository)
{
    public async Task<Post> CreatePostAsync(CreatePostRequest request, CancellationToken ct)
    {
        var visibility = Enum.TryParse<PostVisibility>(request.Visibility, true, out var v)
            ? v : PostVisibility.Public;

        var post = new Post
        {
            AuthorId = request.AuthorId,
            Content = request.Content,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            LocationName = request.LocationName,
            Visibility = visibility,
            PlaceId = request.PlaceId
        };

        if (request.Media is { Count: > 0 })
        {
            foreach (var m in request.Media)
            {
                var mediaType = Enum.TryParse<MediaType>(m.Type, true, out var mt)
                    ? mt : MediaType.Image;

                post.Media.Add(new PostMedia
                {
                    PostId = post.Id,
                    Url = m.Url,
                    Type = mediaType,
                    SortOrder = m.SortOrder,
                    Caption = m.Caption
                });
            }
        }

        await postRepository.AddAsync(post, ct);
        return post;
    }

    public PostResponse MapToResponse(Post post)
    {
        return new PostResponse(
            post.Id,
            post.AuthorId,
            post.Author?.DisplayName ?? "Unknown",
            post.Author?.AvatarUrl,
            post.Content,
            post.Latitude,
            post.Longitude,
            post.LocationName,
            post.Visibility.ToString(),
            post.PlaceId,
            post.Place?.Name,
            post.LikeCount,
            post.CommentCount,
            post.Media.Select(m => new MediaResponse(m.Id, m.Url, m.Type.ToString(), m.SortOrder, m.Caption)).ToList(),
            post.CreatedAt);
    }
}
