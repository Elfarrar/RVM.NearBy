using RVM.NearBy.Domain.Entities;

namespace RVM.NearBy.Domain.Interfaces;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Post?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<List<Post>> GetByAuthorAsync(Guid authorId, int offset, int limit, CancellationToken ct = default);
    Task<List<Post>> GetNearbyAsync(double latitude, double longitude, double radiusKm,
        int offset, int limit, CancellationToken ct = default);
    Task<List<Post>> GetRecentAsync(int offset, int limit, CancellationToken ct = default);
    Task<int> CountByAuthorAsync(Guid authorId, CancellationToken ct = default);
    Task AddAsync(Post post, CancellationToken ct = default);
    Task UpdateAsync(Post post, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
