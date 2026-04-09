using RVM.NearBy.Domain.Entities;

namespace RVM.NearBy.Domain.Interfaces;

public interface IPlaceRepository
{
    Task<Place?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Place>> GetNearbyAsync(double latitude, double longitude, double radiusKm,
        int limit, CancellationToken ct = default);
    Task<List<Place>> SearchAsync(string query, int limit, CancellationToken ct = default);
    Task AddAsync(Place place, CancellationToken ct = default);
    Task UpdateAsync(Place place, CancellationToken ct = default);
}
