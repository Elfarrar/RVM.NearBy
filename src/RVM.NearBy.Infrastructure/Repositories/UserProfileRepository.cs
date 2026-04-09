using Microsoft.EntityFrameworkCore;
using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Domain.Interfaces;
using RVM.NearBy.Infrastructure.Data;

namespace RVM.NearBy.Infrastructure.Repositories;

public class UserProfileRepository(NearByDbContext context) : IUserProfileRepository
{
    public async Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.UserProfiles.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<UserProfile?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await context.UserProfiles.FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<List<UserProfile>> SearchAsync(string? query, int offset, int limit, CancellationToken ct = default)
    {
        var q = context.UserProfiles.Where(u => u.IsActive);

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(u => u.Username.Contains(query) || u.DisplayName.Contains(query));

        return await q.OrderBy(u => u.Username)
            .Skip(offset).Take(limit)
            .ToListAsync(ct);
    }

    public async Task AddAsync(UserProfile profile, CancellationToken ct = default)
    {
        context.UserProfiles.Add(profile);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(UserProfile profile, CancellationToken ct = default)
    {
        context.UserProfiles.Update(profile);
        await context.SaveChangesAsync(ct);
    }
}
