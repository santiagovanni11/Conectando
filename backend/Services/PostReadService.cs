using Conectando.Api.Data;
using Conectando.Api.DTOs.Posts;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public class PostReadService(ConectandoDbContext dbContext, PostVisibilityService visibility) : IPostReadService
{
    public async Task<PostDto> GetByIdAsync(Guid id, Guid viewerId, CancellationToken cancellationToken = default)
    {
        var post = await dbContext.Posts
            .AsNoTracking()
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (post is null || !await visibility.IsVisibleAsync(post, viewerId, cancellationToken))
        {
            throw new PostNotFoundException();
        }

        var media = await LoadMediaAsync(post.Id, cancellationToken);
        var (likeCount, likedByMe) = await PostLikeReader.GetStatusAsync(dbContext, post.Id, viewerId, cancellationToken);
        var commentCount = CommentCountService.CountCommentsByPosts(dbContext.Comments, new[] { post.Id }).GetValueOrDefault(post.Id, 0);
        return PostDtoMapping.ToDto(post, post.Author, media, likeCount, likedByMe, commentCount);
    }

    public async Task<PostListDto> ListByUserAsync(Guid authorId, Guid viewerId, PostListQuery query, CancellationToken cancellationToken = default)
    {
        if (!await dbContext.Users.AnyAsync(u => u.Id == authorId, cancellationToken))
        {
            throw new UserNotFoundException();
        }

        var limit = Math.Clamp(query.Limit, 1, 20);
        var blockedIds = await visibility.GetBlockedIdsAsync(viewerId, cancellationToken);
        var friendIds = await visibility.GetFriendIdsAsync(viewerId, cancellationToken);

        IQueryable<Post> baseQuery = dbContext.Posts
            .AsNoTracking()
            .Include(p => p.Author)
            .Where(p => p.AuthorId == authorId);

        if (blockedIds.Count > 0)
        {
            baseQuery = baseQuery.Where(p => !blockedIds.Contains(p.AuthorId));
        }

        if (viewerId != authorId)
        {
            baseQuery = baseQuery.Where(p => p.Privacy == PostPrivacy.Public
                || (p.Privacy == PostPrivacy.Friends && friendIds.Contains(p.AuthorId)));
        }

        var cursor = PostListCursor.TryDecode(query.Cursor);
        if (cursor is not null)
        {
            baseQuery = baseQuery.Where(p => p.CreatedAt < cursor.Value.CreatedAt
                || (p.CreatedAt == cursor.Value.CreatedAt && p.Id < cursor.Value.Id));
        }

        baseQuery = baseQuery
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id);

        var items = await baseQuery.Take(limit + 1).ToListAsync(cancellationToken);

        var hasMore = items.Count > limit;
        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
        }

        var ids = items.Select(p => p.Id).ToList();
        var media = await dbContext.PostMedia
            .AsNoTracking()
            .Where(m => m.PostId != null && ids.Contains(m.PostId.Value) && m.State == PostMediaState.Attached)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);

        var byPost = media.GroupBy(m => m.PostId!.Value).ToDictionary(g => g.Key, g => g.ToList());
        var likeCounts = await PostLikeReader.CountByPostsAsync(dbContext, ids, cancellationToken);
        var likedByMeIds = await PostLikeReader.LikedByUserAsync(dbContext, ids, viewerId, cancellationToken);
        var savedByMeIds = await PostLikeReader.SavedByUserAsync(dbContext, ids, viewerId, cancellationToken);
        var commentCounts = CommentCountService.CountCommentsByPosts(dbContext.Comments, ids).ToDictionary(x => x.Key, x => x.Value);
        var nextCursor = hasMore && items.Count > 0 ? PostListCursor.Encode(items[^1]) : null;

        return new PostListDto
        {
            Items = items.Select(p => PostDtoMapping.ToDto(p, p.Author,
                byPost.TryGetValue(p.Id, out var list) ? list : [],
                likeCounts.GetValueOrDefault(p.Id),
                likedByMeIds.Contains(p.Id),
                commentCounts.GetValueOrDefault(p.Id, 0),
                savedByMeIds.Contains(p.Id))).ToList(),
            NextCursor = nextCursor,
            HasMore = hasMore,
        };
    }

    private async Task<List<PostMedia>> LoadMediaAsync(Guid postId, CancellationToken cancellationToken)
    {
        return await dbContext.PostMedia
            .AsNoTracking()
            .Where(m => m.PostId == postId && m.State == PostMediaState.Attached)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);
    }
}