using Conectando.Api.DTOs.Notifications;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Creación y marcado de notificaciones.
/// </summary>
/// <remarks>
/// Todas las escrituras terminan avisando a la navegacion, porque cada una
/// cambia el número que se ve arriba de la campanita. Si se olvidara, el
/// número seguiría en pantalla hasta el siguiente refresco, y el usuario
/// creería que ya lo vio.
/// </remarks>
public partial class NotificationService
{
    public async Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientId == userId, cancellationToken);

        if (notification is null) throw new NotificationNotFoundException();
        if (notification.ReadAt is not null) return;

        notification.ReadAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await navCounts.NotifyAsync(userId, cancellationToken);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var pending = await dbContext.Notifications
            .Where(n => n.RecipientId == userId && n.ReadAt == null)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0) return;

        var now = DateTime.UtcNow;
        foreach (var notification in pending)
        {
            notification.ReadAt = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await navCounts.NotifyAsync(userId, cancellationToken);
    }

    public async Task NotifyAsync(Guid recipientId, Guid actorId, NotificationTypeRequest type, Guid? postId, CancellationToken cancellationToken = default)
    {
        // No avisarse a uno mismo ni duplicar la misma interacción.
        if (recipientId == actorId) return;

        var alreadyExists = await dbContext.Notifications.AnyAsync(
            n => n.RecipientId == recipientId
                 && n.ActorId == actorId
                 && n.Type == (NotificationType)type
                 && n.PostId == postId,
            cancellationToken);

        if (alreadyExists) return;

        dbContext.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            RecipientId = recipientId,
            ActorId = actorId,
            Type = (NotificationType)type,
            PostId = postId,
            CreatedAt = DateTime.UtcNow,
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        // Recién después de guardar: si se avisara antes, el contador
        // mostraria uno por detras de lo que el usuario ya tiene guardado.
        await navCounts.NotifyAsync(recipientId, cancellationToken);
    }
}
