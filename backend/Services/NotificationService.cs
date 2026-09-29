using Conectando.Api.Data;
using Conectando.Api.DTOs.Notifications;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace Conectando.Api.Services;

/// <summary>
/// Lectura de las notificaciones.
/// </summary>
/// <remarks>
/// Partido en dos porque son dos caminos distintos
/// <c>NotificationService.Write</c> crea y marca como leído. Juntos pasaron de
/// 150 líneas, y además las escrituras son las que necesitan avisar a la
/// navegación, así que conviene que se vean.
/// </remarks>
public partial class NotificationService(ConectandoDbContext dbContext, INavCountsBroadcaster navCounts)
    : INotificationService
{
    private const int MaxLimit = 50;

    public async Task<NotificationPageDto> GetAllAsync(Guid userId, string? cursor, int limit, CancellationToken cancellationToken = default)
    {
        var take = Math.Clamp(limit, 1, MaxLimit);
        var parsed = NotificationCursorParser.Parse(cursor);

        var query = dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientId == userId);

        if (parsed is not null)
        {
            query = query.Where(n =>
                n.CreatedAt < parsed.CreatedAt ||
                (n.CreatedAt == parsed.CreatedAt && n.Id.CompareTo(parsed.Id) < 0));
        }

        var rows = await query
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .Take(take + 1)
            .Select(n => new NotificationRow(
                n.Id,
                n.Type,
                n.CreatedAt,
                n.ReadAt,
                n.PostId,
                n.ActorId,
                n.Actor.UserName,
                n.Actor.DisplayName,
                n.Actor.ProfileImageUrl))
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > take;
        var page = hasMore ? rows[..take] : rows;

        var unreadCount = await dbContext.Notifications
            .CountAsync(n => n.RecipientId == userId && n.ReadAt == null, cancellationToken);

        return new NotificationPageDto
        {
            Items = page.Select(Map).ToList(),
            HasMore = hasMore,
            UnreadCount = unreadCount,
            NextCursor = hasMore && page.Count > 0
                ? NotificationCursorParser.Encode(page[^1].CreatedAt, page[^1].Id)
                : null,
        };
    }

    public async Task<UnreadCountDto> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var count = await dbContext.Notifications
            .CountAsync(n => n.RecipientId == userId && n.ReadAt == null, cancellationToken);

        return new UnreadCountDto { Count = count };
    }

    private static NotificationDto Map(NotificationRow row) => new()
    {
        Id = row.Id,
        Type = row.Type.ToString(),
        IsRead = row.ReadAt is not null,
        CreatedAt = row.CreatedAt,
        PostId = row.PostId,
        Actor = new NotificationActorDto
        {
            Id = row.ActorId,
            UserName = row.ActorUserName,
            DisplayName = row.ActorDisplayName,
            ProfileImageUrl = row.ActorProfileImageUrl,
        },
    };
}
