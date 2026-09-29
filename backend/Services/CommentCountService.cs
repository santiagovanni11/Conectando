using Conectando.Api.Data;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public static class CommentCountService
{
    public static async Task<Dictionary<Guid, int>> GetRepliesCountAsync(ConectandoDbContext dbContext, IReadOnlyList<Guid> commentIds, CancellationToken cancellationToken)
    {
        if (commentIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        return await dbContext.Comments
            .AsNoTracking()
            .Where(c => c.ParentCommentId != null && commentIds.Contains(c.ParentCommentId!.Value))
            .GroupBy(c => c.ParentCommentId!.Value)
            .Select(g => new { CommentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CommentId, x => x.Count, cancellationToken);
    }

    public static Dictionary<Guid, int> CountCommentsByPosts(IQueryable<Comment> comments, IReadOnlyList<Guid> postIds)
    {
        if (postIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        return comments
            .Where(c => postIds.Contains(c.PostId) && c.ParentCommentId == null && c.DeletedAt == null)
            .GroupBy(c => c.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count() })
            .ToDictionary(x => x.PostId, x => x.Count);
    }
}