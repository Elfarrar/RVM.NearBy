using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Domain.Enums;

namespace RVM.NearBy.Test.Domain;

public class EntityTests
{
    [Fact]
    public void UserProfile_Defaults_AreCorrect()
    {
        var profile = new UserProfile();
        Assert.NotEqual(Guid.Empty, profile.Id);
        Assert.Equal(string.Empty, profile.Username);
        Assert.Equal(string.Empty, profile.DisplayName);
        Assert.True(profile.IsActive);
        Assert.Null(profile.Bio);
        Assert.Null(profile.AvatarUrl);
        Assert.Null(profile.LastLatitude);
        Assert.Null(profile.LastLongitude);
    }

    [Fact]
    public void UserProfile_Properties_SetCorrectly()
    {
        var profile = new UserProfile
        {
            Username = "john",
            DisplayName = "John Doe",
            Bio = "Hello",
            AvatarUrl = "https://example.com/avatar.png",
            LastLatitude = -23.55,
            LastLongitude = -46.63
        };

        Assert.Equal("john", profile.Username);
        Assert.Equal("John Doe", profile.DisplayName);
        Assert.Equal("Hello", profile.Bio);
        Assert.Equal(-23.55, profile.LastLatitude);
        Assert.Equal(-46.63, profile.LastLongitude);
    }

    [Fact]
    public void Post_Defaults_AreCorrect()
    {
        var post = new Post();
        Assert.NotEqual(Guid.Empty, post.Id);
        Assert.Equal(string.Empty, post.Content);
        Assert.Equal(PostVisibility.Public, post.Visibility);
        Assert.Equal(0, post.LikeCount);
        Assert.Equal(0, post.CommentCount);
        Assert.Null(post.PlaceId);
        Assert.Empty(post.Media);
        Assert.Empty(post.Comments);
        Assert.Empty(post.Likes);
    }

    [Fact]
    public void Post_Properties_SetCorrectly()
    {
        var authorId = Guid.NewGuid();
        var placeId = Guid.NewGuid();
        var post = new Post
        {
            AuthorId = authorId,
            Content = "Hello from SP!",
            Latitude = -23.55,
            Longitude = -46.63,
            LocationName = "Sao Paulo",
            Visibility = PostVisibility.NearbyOnly,
            PlaceId = placeId
        };

        Assert.Equal(authorId, post.AuthorId);
        Assert.Equal("Hello from SP!", post.Content);
        Assert.Equal(-23.55, post.Latitude);
        Assert.Equal(PostVisibility.NearbyOnly, post.Visibility);
        Assert.Equal(placeId, post.PlaceId);
    }

    [Fact]
    public void PostMedia_Defaults_AreCorrect()
    {
        var media = new PostMedia();
        Assert.NotEqual(Guid.Empty, media.Id);
        Assert.Equal(string.Empty, media.Url);
        Assert.Equal(MediaType.Image, media.Type);
        Assert.Equal(0, media.SortOrder);
        Assert.Null(media.Caption);
    }

    [Fact]
    public void Comment_Defaults_AreCorrect()
    {
        var comment = new Comment();
        Assert.NotEqual(Guid.Empty, comment.Id);
        Assert.Equal(string.Empty, comment.Content);
    }

    [Fact]
    public void Like_Defaults_AreCorrect()
    {
        var like = new Like();
        Assert.NotEqual(Guid.Empty, like.Id);
    }

    [Fact]
    public void Place_Defaults_AreCorrect()
    {
        var place = new Place();
        Assert.NotEqual(Guid.Empty, place.Id);
        Assert.Equal(string.Empty, place.Name);
        Assert.Equal(0, place.PostCount);
        Assert.Null(place.Description);
        Assert.Null(place.Category);
        Assert.Null(place.Address);
        Assert.Empty(place.Posts);
    }

    [Fact]
    public void PostVisibility_HasExpectedValues()
    {
        Assert.Equal(0, (int)PostVisibility.Public);
        Assert.Equal(1, (int)PostVisibility.NearbyOnly);
        Assert.Equal(2, (int)PostVisibility.Private);
    }

    [Fact]
    public void MediaType_HasExpectedValues()
    {
        Assert.Equal(0, (int)MediaType.Image);
        Assert.Equal(1, (int)MediaType.Video);
        Assert.Equal(2, (int)MediaType.Audio);
    }
}
