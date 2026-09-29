using Conectando.Api.Data;
using Conectando.Api.DTOs.Posts;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>Fila del listado de "quién dio me gusta", con la fecha para el cursor.</summary>
internal sealed record LikeUserRow(
    Guid Id,
    DateTime CreatedAt,
    string UserName,
    string DisplayName,
    string? ProfileImageUrl);

public class LikeService(ConectandoDbContext dbContext, PostVisibilityService visibility, INotificationService notifications) : ILikeService
{
    public async Task<PostLikeStatusDto> LikeAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default)
    {
        var post = await LoadVisiblePostAsync(postId, userId, cancellationToken);

        dbContext.Likes.Add(new Like { UserId = userId, PostId = postId, CreatedAt = DateTime.UtcNow });

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await notifications.NotifyAsync(post.AuthorId, userId, NotificationTypeRequest.Like, postId, cancellationToken);
        }
        catch (DbUpdateException exception) when (PostgresErrors.IsUniqueViolation(exception))
        {
            // Solicitud simultánea: el like ya existía. Idempotente.
        }

        var (count, liked) = await PostLikeReader.GetStatusAsync(dbContext, postId, userId, cancellationToken);
        return new PostLikeStatusDto { LikesCount = count, LikedByMe = liked };
    }

    public async Task<PostLikeStatusDto> UnlikeAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default)
    {
        var post = await LoadVisiblePostAsync(postId, userId, cancellationToken);

        var like = await dbContext.Likes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId, cancellationToken);

        if (like is not null)
        {
            dbContext.Likes.Remove(like);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var (count, liked) = await PostLikeReader.GetStatusAsync(dbContext, postId, userId, cancellationToken);
        return new PostLikeStatusDto { LikesCount = count, LikedByMe = liked };
    }

    public async Task<PostLikeStatusDto> GetStatusAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default)
    {
        await LoadVisiblePostAsync(postId, userId, cancellationToken);

        var (count, liked) = await PostLikeReader.GetStatusAsync(dbContext, postId, userId, cancellationToken);
        return new PostLikeStatusDto { LikesCount = count, LikedByMe = liked };
    }

    public async Task<LikePageDto> GetLikesAsync(
        Guid postId,
        Guid userId,
        string? cursor,
        int limit,
        CancellationToken cancellationToken = default)
    {
        await LoadVisiblePostAsync(postId, userId, cancellationToken);

        var take = Math.Clamp(limit, 1, 100);
        var parsed = LikeCursorParser.Parse(cursor);

        var query = dbContext.Likes
            .AsNoTracking()
            .Where(l => l.PostId == postId)
            .Select(l => l.User);

        if (parsed is not null)
        {
            query = query.Where(u =>
                u.CreatedAt < parsed.CreatedAt ||
                (u.CreatedAt == parsed.CreatedAt && u.Id.CompareTo(parsed.UserId) < 0));
        }

        var rows = await query
            .OrderByDescending(u => u.CreatedAt)
            .ThenByDescending(u => u.Id)
            .Take(take + 1)
            .Select(u => new LikeUserRow(u.Id, u.CreatedAt, u.UserName, u.DisplayName, u.ProfileImageUrl))
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > take;
        var page = hasMore ? rows[..take] : rows;

        return new LikePageDto
        {
            HasMore = hasMore,
            Items = [.. page.Select(r => new LikeUserDto
            {
                Id = r.Id,
                UserName = r.UserName,
                DisplayName = r.DisplayName,
                ProfileImageUrl = r.ProfileImageUrl,
            })],
            // El cursor lleva la fecha real de la fila, no la de ahora:
            // si no, la paginación se rompería al segundo corte.
            NextCursor = hasMore && page.Count > 0
                ? LikeCursorParser.Encode(page[^1].CreatedAt, page[^1].Id)
                : null,
        };
    }

    /// <summary>
    /// Un like solo existe sobre un post que el usuario puede ver. Por eso
    /// se valida con el mismo servicio de visibilidad que usa el feed.
    /// </summary>
    private async Task<Post> LoadVisiblePostAsync(Guid postId, Guid userId, CancellationToken cancellationToken)
    {
        var post = await dbContext.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post is null || !await visibility.IsVisibleAsync(post, userId, cancellationToken))
        {
            throw new PostNotFoundException();
        }

        return post;
    }
}