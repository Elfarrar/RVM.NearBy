using RVM.NearBy.Domain.Entities;

namespace RVM.NearBy.Domain.Interfaces;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserProfile?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<List<UserProfile>> SearchAsync(string? query, int offset, int limit, CancellationToken ct = default);
    Task AddAsync(UserProfile profile, CancellationToken ct = default);
    Task UpdateAsync(UserProfile profile, CancellationToken ct = default);
}
