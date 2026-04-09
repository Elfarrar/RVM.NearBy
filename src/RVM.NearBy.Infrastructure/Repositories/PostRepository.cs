using Microsoft.EntityFrameworkCore;
using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Domain.Enums;
using RVM.NearBy.Domain.Interfaces;
using RVM.NearBy.Infrastructure.Data;

namespace RVM.NearBy.Infrastructure.Repositories;

public class PostRepository(NearByDbContext context) : IPostRepository
{
    public async Task<Post?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Posts.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Post?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await context.Posts
            .Include(p => p.Author)
            .Include(p => p.Media)
            .Include(p => p.Place)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<List<Post>> GetByAuthorAsync(Guid authorId, int offset, int limit, CancellationToken ct = default)
        => await context.Posts
            .Where(p => p.AuthorId == authorId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(offset).Take(limit)
            .Include(p => p.Author)
            .ToListAsync(ct);

    public async Task<List<Post>> GetNearbyAsync(double latitude, double longitude, double radiusKm,
        int offset, int limit, CancellationToken ct = default)
    {
        // Haversine-compatible filtering using simple bounding box + in-memory distance
        var latDelta = radiusKm / 111.0;
        var lngDelta = radiusKm / (111.0 * Math.Cos(latitude * Math.PI / 180.0));

        var candidates = await context.Posts
            .Where(p => p.Visibility == PostVisibility.Public || p.Visibility == PostVisibility.NearbyOnly)
            .Where(p => p.Latitude >= latitude - latDelta && p.Latitude <= latitude + latDelta)
            .Where(p => p.Longitude >= longitude - lngDelta && p.Longitude <= longitude + lngDelta)
            .Include(p => p.Author)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        return candidates
            .Where(p => CalculateDistanceKm(latitude, longitude, p.Latitude, p.Longitude) <= radiusKm)
            .Skip(offset).Take(limit)
            .ToList();
    }

    public async Task<List<Post>> GetRecentAsync(int offset, int limit, CancellationToken ct = default)
        => await context.Posts
            .Where(p => p.Visibility == PostVisibility.Public)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(offset).Take(limit)
            .Include(p => p.Author)
            .ToListAsync(ct);

    public async Task<int> CountByAuthorAsync(Guid authorId, CancellationToken ct = default)
        => await context.Posts.CountAsync(p => p.AuthorId == authorId, ct);

    public async Task AddAsync(Post post, CancellationToken ct = default)
    {
        context.Posts.Add(post);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Post post, CancellationToken ct = default)
    {
        context.Posts.Update(post);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var post = await context.Posts.FindAsync([id], ct);
        if (post is not null)
        {
            context.Posts.Remove(post);
            await context.SaveChangesAsync(ct);
        }
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
