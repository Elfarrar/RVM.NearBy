namespace RVM.NearBy.API.Dtos;

public record CreateProfileRequest(string Username, string DisplayName, string? Bio, string? AvatarUrl);
public record UpdateProfileRequest(string? DisplayName, string? Bio, string? AvatarUrl);
public record UpdateLocationRequest(double Latitude, double Longitude);

public record ProfileResponse(
    Guid Id,
    string Username,
    string DisplayName,
    string? Bio,
    string? AvatarUrl,
    bool IsActive,
    int PostCount,
    DateTime CreatedAt);
