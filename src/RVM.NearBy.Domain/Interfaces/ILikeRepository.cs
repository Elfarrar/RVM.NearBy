using RVM.NearBy.Domain.Entities;

namespace RVM.NearBy.Domain.Interfaces;

public interface ILikeRepository
{
    Task<Like?> GetByPostAndUserAsync(Guid postId, Guid userId, CancellationToken ct = default);
    Task<int> CountByPostIdAsync(Guid postId, CancellationToken ct = default);
    Task AddAsync(Like like, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
