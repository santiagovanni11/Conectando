using Conectando.Api.Data;
using Conectando.Api.DTOs.Counters;
using Conectando.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Cuenta los avisos de la navegación en una sola consulta por tipo.
/// Se llama seguido al entrar y cuando llega algo por el hub, así que
/// tiene que ser barato.
/// </summary>
public class NavCountService(ConectandoDbContext dbContext) : INavCountService
{
    private readonly ConectandoDbContext _dbContext = dbContext;

    public async Task<NavCountsDto> GetCountsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return new NavCountsDto
        {
            UnreadMessages = await UnreadMessagesAsync(userId, cancellationToken),
            PendingFriendRequests = await PendingRequestsAsync(userId, cancellationToken),
            UnreadNotifications = await UnreadNotificationsAsync(userId, cancellationToken),
        };
    }

    /// <summary>
    /// Mensajes de los otros que llegaron después de la última lectura de
    /// cada conversación.
    /// </summary>
    private async Task<int> UnreadMessagesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var lastRead = await _dbContext.ConversationMembers
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Select(m => new { m.ConversationId, m.LastReadAt })
            .ToListAsync(cancellationToken);

        if (lastRead.Count == 0) return 0;

        var conversationIds = lastRead.Select(m => m.ConversationId).ToList();
        var cutoffs = lastRead.ToDictionary(m => m.ConversationId, m => m.LastReadAt ?? DateTime.MinValue);

        var incoming = await _dbContext.Messages
            .AsNoTracking()
            .Where(m => conversationIds.Contains(m.ConversationId) && m.SenderId != userId)
            .Select(m => new { m.ConversationId, m.CreatedAt })
            .ToListAsync(cancellationToken);

        return incoming.Count(m => m.CreatedAt > cutoffs[m.ConversationId]);
    }

    // La tabla no tiene columna de estado: una solicitud sigue pendiente
    // mientras el par no exista en friendships (lo resuelve el servicio).
    private async Task<int> PendingRequestsAsync(Guid userId, CancellationToken cancellationToken) =>
        await _dbContext.FriendRequests
            .AsNoTracking()
            .CountAsync(r => r.AddresseeId == userId, cancellationToken);

    private async Task<int> UnreadNotificationsAsync(Guid userId, CancellationToken cancellationToken) =>
        await _dbContext.Notifications
            .AsNoTracking()
            .CountAsync(n => n.RecipientId == userId && n.ReadAt == null, cancellationToken);
}