using Conectando.Api.DTOs.Notifications;
using Conectando.Api.Interfaces;

namespace Conectando.Api.Tests.TestInfrastructure;

/// <summary>
/// Notificador descartable: los tests verifican el efecto en la base,
/// no la creación de notificaciones.
/// </summary>
public sealed class NullNotificationService : INotificationService
{
    public Task<NotificationPageDto> GetAllAsync(Guid userId, string? cursor, int limit, CancellationToken cancellationToken = default)
        => Task.FromResult(new NotificationPageDto());

    public Task<UnreadCountDto> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
        => Task.FromResult(new UnreadCountDto());

    public Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task NotifyAsync(Guid recipientId, Guid actorId, NotificationTypeRequest type, Guid? postId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}