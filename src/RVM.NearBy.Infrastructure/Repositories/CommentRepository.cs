using Microsoft.EntityFrameworkCore;
using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Domain.Interfaces;
using RVM.NearBy.Infrastructure.Data;

namespace RVM.NearBy.Infrastructure.Repositories;

public class CommentRepository(NearByDbContext context) : ICommentRepository
{
    public async Task<List<Comment>> GetByPostIdAsync(Guid postId, int offset, int limit, CancellationToken ct = default)
        => await context.Comments
            .Where(c => c.PostId == postId)
            .OrderByDescending(c => c.CreatedAt)
            .Skip(offset).Take(limit)
            .Include(c => c.Author)
            .ToListAsync(ct);

    public async Task<int> CountByPostIdAsync(Guid postId, CancellationToken ct = default)
        => await context.Comments.CountAsync(c => c.PostId == postId, ct);

    public async Task AddAsync(Comment comment, CancellationToken ct = default)
    {
        context.Comments.Add(comment);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var comment = await context.Comments.FindAsync([id], ct);
        if (comment is not null)
        {
            context.Comments.Remove(comment);
            await context.SaveChangesAsync(ct);
        }
    }
}
