using Conectando.Api.Data;
using Conectando.Api.DTOs.Posts;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Feed query-time: una sola consulta de posts contra el conjunto de autores
/// permitidos + batch de media y likes. Sin tablas materializadas ni N+1.
/// </summary>
public class FeedReadService(ConectandoDbContext dbContext, FeedAuthorResolver authors) : IFeedService
{
    public async Task<PostListDto> GetPageAsync(
        Guid viewerId, string? cursor, int limit, CancellationToken cancellationToken = default)
    {
        var take = Math.Clamp(limit, 1, 20);
        var context = await authors.ResolveAsync(dbContext, viewerId, cancellationToken);
        var authorIds = context.AuthorIds.ToList();

        var query = dbContext.Posts
            .AsNoTracking()
            .Include(p => p.Author)
            .Where(p => authorIds.Contains(p.AuthorId))
            .Where(p => p.AuthorId == viewerId
                || p.Privacy == PostPrivacy.Public
                || (p.Privacy == PostPrivacy.Friends && context.FriendIds.Contains(p.AuthorId)));

        var position = PostListCursor.TryDecode(cursor);
        if (cursor is not null && position is null)
        {
            throw new InvalidCursorException();
        }

        if (position is not null)
        {
            query = query.Where(p => p.CreatedAt < position.Value.CreatedAt
                || (p.CreatedAt == position.Value.CreatedAt && p.Id < position.Value.Id));
        }

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Take(take + 1)
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > take;
        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
        }

        var page = await BuildPageAsync(items, viewerId, cancellationToken);
        return new PostListDto
        {
            Items = page,
            NextCursor = hasMore && items.Count > 0 ? PostListCursor.Encode(items[^1]) : null,
            HasMore = hasMore,
        };
    }

    private async Task<List<PostDto>> BuildPageAsync(List<Post> posts, Guid viewerId, CancellationToken cancellationToken)
    {
        if (posts.Count == 0)
        {
            return [];
        }

        var ids = posts.Select(p => p.Id).ToList();
        var media = await dbContext.PostMedia
            .AsNoTracking()
            .Where(m => m.PostId != null && ids.Contains(m.PostId.Value) && m.State == PostMediaState.Attached)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);
        var byPost = media.GroupBy(m => m.PostId!.Value).ToDictionary(g => g.Key, g => g.ToList());

        var likeCounts = await PostLikeReader.CountByPostsAsync(dbContext, ids, cancellationToken);
        var likedByMeIds = await PostLikeReader.LikedByUserAsync(dbContext, ids, viewerId, cancellationToken);
        var commentCounts = CommentCountService.CountCommentsByPosts(dbContext.Comments, ids);

        return posts.Select(p => PostDtoMapping.ToDto(p, p.Author,
            byPost.TryGetValue(p.Id, out var list) ? list : [],
            likeCounts.GetValueOrDefault(p.Id),
            likedByMeIds.Contains(p.Id),
            commentCounts.GetValueOrDefault(p.Id))).ToList();
    }
}
