using RVM.NearBy.Domain.Entities;

namespace RVM.NearBy.Domain.Interfaces;

public interface ICommentRepository
{
    Task<List<Comment>> GetByPostIdAsync(Guid postId, int offset, int limit, CancellationToken ct = default);
    Task<int> CountByPostIdAsync(Guid postId, CancellationToken ct = default);
    Task AddAsync(Comment comment, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
