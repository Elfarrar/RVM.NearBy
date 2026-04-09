using Microsoft.EntityFrameworkCore;
using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Domain.Interfaces;
using RVM.NearBy.Infrastructure.Data;

namespace RVM.NearBy.Infrastructure.Repositories;

public class PlaceRepository(NearByDbContext context) : IPlaceRepository
{
    public async Task<Place?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Places.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<List<Place>> GetNearbyAsync(double latitude, double longitude, double radiusKm,
        int limit, CancellationToken ct = default)
    {
        var latDelta = radiusKm / 111.0;
        var lngDelta = radiusKm / (111.0 * Math.Cos(latitude * Math.PI / 180.0));

        var candidates = await context.Places
            .Where(p => p.Latitude >= latitude - latDelta && p.Latitude <= latitude + latDelta)
            .Where(p => p.Longitude >= longitude - lngDelta && p.Longitude <= longitude + lngDelta)
            .ToListAsync(ct);

        return candidates
            .Where(p => CalculateDistanceKm(latitude, longitude, p.Latitude, p.Longitude) <= radiusKm)
            .OrderBy(p => CalculateDistanceKm(latitude, longitude, p.Latitude, p.Longitude))
            .Take(limit)
            .ToList();
    }

    public async Task<List<Place>> SearchAsync(string query, int limit, CancellationToken ct = default)
        => await context.Places
            .Where(p => p.Name.Contains(query) || (p.Category != null && p.Category.Contains(query)))
            .OrderBy(p => p.Name)
            .Take(limit)
            .ToListAsync(ct);

    public async Task AddAsync(Place place, CancellationToken ct = default)
    {
        context.Places.Add(place);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Place place, CancellationToken ct = default)
    {
        context.Places.Update(place);
        await context.SaveChangesAsync(ct);
    }

    private static double CalculateDistanceKm(double lat1, double lng1, double lat2, double lng2)
    {
        const double R = 6371.0;
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLng = (lng2 - lng1) * Math.PI / 180.0;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return R * 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
