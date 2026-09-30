using Conectando.Api.Data;
using Conectando.Api.DTOs.Comments;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public class CommentReadService(ConectandoDbContext dbContext, PostVisibilityService visibility)
{
    public async Task<CommentPageDto> GetCommentsAsync(Guid postId, Guid userId, string? cursor, int limit, CancellationToken cancellationToken = default)
    {
        var post = await dbContext.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post is null || !await visibility.IsVisibleAsync(post, userId, cancellationToken))
        {
            throw new PostNotFoundException();
        }

        var parsedCursor = CommentCursorParser.Parse(cursor);
        var pageSize = Math.Clamp(limit, 1, 20);
        var fetchCount = pageSize + 1;

        var query = dbContext.Comments
            .AsNoTracking()
            .Where(c => c.PostId == postId && c.ParentCommentId == null && c.DeletedAt == null);

        if (parsedCursor is not null)
        {
            query = query.Where(c =>
                c.CreatedAt > parsedCursor.CreatedAt ||
                (c.CreatedAt == parsedCursor.CreatedAt && c.Id > parsedCursor.Id));
        }

        var comments = await query
            .OrderBy(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .Take(fetchCount)
            .ToListAsync(cancellationToken);

        var hasMore = comments.Count > pageSize;
        var pageItems = comments.Take(pageSize).ToList();

        var commentIds = pageItems.Select(c => c.Id).ToList();
        var repliesCount = await CommentCountService.GetRepliesCountAsync(dbContext, commentIds, cancellationToken);
        var likes = await CommentLikeReader.GetByCommentsAsync(dbContext, commentIds, userId, cancellationToken);

        var dtos = await CommentDtoProjector.ProjectAsync(dbContext, pageItems, cancellationToken);
        foreach (var dto in dtos)
        {
            dto.RepliesCount = repliesCount.GetValueOrDefault(dto.Id, 0);
            var estado = likes.GetValueOrDefault(dto.Id);
            dto.LikesCount = estado.Count;
            dto.LikedByMe = estado.LikedByMe;
        }

        var last = pageItems.LastOrDefault();
        var nextCursor = hasMore && last is not null
            ? CommentCursorParser.Encode(last.CreatedAt, last.Id)
            : null;

        return new CommentPageDto
        {
            Items = dtos,
            HasMore = hasMore,
            NextCursor = nextCursor,
        };
    }

    public async Task<IReadOnlyList<CommentDto>> GetRepliesAsync(Guid parentCommentId, Guid userId, int limit, CancellationToken cancellationToken = default)
    {
        var parent = await dbContext.Comments
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == parentCommentId, cancellationToken);

        if (parent is null)
        {
            throw new CommentNotFoundException();
        }

        var post = await dbContext.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == parent.PostId, cancellationToken);

        if (post is null || !await visibility.IsVisibleAsync(post, userId, cancellationToken))
        {
            throw new PostNotFoundException();
        }

        var replies = await dbContext.Comments
            .AsNoTracking()
            .Where(c => c.ParentCommentId == parentCommentId)
            .OrderBy(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var likes = await CommentLikeReader.GetByCommentsAsync(
            dbContext, replies.Select(c => c.Id).ToList(), userId, cancellationToken);

        var dtos = await CommentDtoProjector.ProjectAsync(dbContext, replies, cancellationToken);
        foreach (var dto in dtos)
        {
            var estado = likes.GetValueOrDefault(dto.Id);
            dto.LikesCount = estado.Count;
            dto.LikedByMe = estado.LikedByMe;
        }

        return dtos;
    }
}