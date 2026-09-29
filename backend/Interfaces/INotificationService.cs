using Conectando.Api.DTOs.Notifications;

namespace Conectando.Api.Interfaces;

public interface INotificationService
{
    Task<NotificationPageDto> GetAllAsync(Guid userId, string? cursor, int limit, CancellationToken cancellationToken = default);
    Task<UnreadCountDto> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task NotifyAsync(Guid recipientId, Guid actorId, NotificationTypeRequest type, Guid? postId, CancellationToken cancellationToken = default);
}

public enum NotificationTypeRequest
{
    Like = 1,
    Comment = 2,
    FriendRequest = 3,
    FriendAccepted = 4,
    Follow = 5,
}