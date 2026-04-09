namespace RVM.NearBy.API.Dtos;

public record CreatePlaceRequest(string Name, string? Description, string? Category, double Latitude, double Longitude, string? Address);
public record UpdatePlaceRequest(string? Name, string? Description, string? Category, string? Address);

public record PlaceResponse(
    Guid Id,
    string Name,
    string? Description,
    string? Category,
    double Latitude,
    double Longitude,
    string? Address,
    int PostCount,
    DateTime CreatedAt);
