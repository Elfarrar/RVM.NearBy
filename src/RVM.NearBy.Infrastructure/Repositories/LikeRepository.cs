using Microsoft.EntityFrameworkCore;
using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Domain.Interfaces;
using RVM.NearBy.Infrastructure.Data;

namespace RVM.NearBy.Infrastructure.Repositories;

public class LikeRepository(NearByDbContext context) : ILikeRepository
{
    public async Task<Like?> GetByPostAndUserAsync(Guid postId, Guid userId, CancellationToken ct = default)
        => await context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId, ct);

    public async Task<int> CountByPostIdAsync(Guid postId, CancellationToken ct = default)
        => await context.Likes.CountAsync(l => l.PostId == postId, ct);

    public async Task AddAsync(Like like, CancellationToken ct = default)
    {
        context.Likes.Add(like);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var like = await context.Likes.FindAsync([id], ct);
        if (like is not null)
        {
            context.Likes.Remove(like);
            await context.SaveChangesAsync(ct);
        }
    }
}
