using Conectando.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public static class PostLikeReader
{
    public static async Task<(int Count, bool LikedByMe)> GetStatusAsync(
        ConectandoDbContext db, Guid postId, Guid viewerId, CancellationToken cancellationToken = default)
    {
        var count = await db.Likes.AsNoTracking().CountAsync(l => l.PostId == postId, cancellationToken);
        var liked = await db.Likes.AsNoTracking().AnyAsync(
            l => l.PostId == postId && l.UserId == viewerId, cancellationToken);

        return (count, liked);
    }

    public static async Task<Dictionary<Guid, int>> CountByPostsAsync(
        ConectandoDbContext db, List<Guid> postIds, CancellationToken cancellationToken = default)
    {
        if (postIds.Count == 0)
        {
            return [];
        }

        return await db.Likes.AsNoTracking()
            .Where(l => postIds.Contains(l.PostId))
            .GroupBy(l => l.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PostId, x => x.Count, cancellationToken);
    }

    public static async Task<HashSet<Guid>> SavedByUserAsync(
        ConectandoDbContext db, List<Guid> postIds, Guid userId, CancellationToken cancellationToken = default)
    {
        if (postIds.Count == 0)
        {
            return [];
        }

        var ids = await db.PostSaves.AsNoTracking()
            .Where(s => postIds.Contains(s.PostId) && s.UserId == userId)
            .Select(s => s.PostId)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }

    public static async Task<HashSet<Guid>> LikedByUserAsync(
        ConectandoDbContext db, List<Guid> postIds, Guid userId, CancellationToken cancellationToken = default)
    {
        if (postIds.Count == 0)
        {
            return [];
        }

        var ids = await db.Likes.AsNoTracking()
            .Where(l => postIds.Contains(l.PostId) && l.UserId == userId)
            .Select(l => l.PostId)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }
}