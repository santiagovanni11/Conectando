using Conectando.Api.Data;
using Conectando.Api.DTOs.Messages;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>Consultas de apoyo para armar la lista de conversaciones.</summary>
public partial class ConversationService
{
    private Task<List<Guid>> PeerIdsAsync(Guid conversationId, Guid excludeUserId, CancellationToken cancellationToken) =>
        _dbContext.ConversationMembers
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId && m.UserId != excludeUserId)
            .Select(m => m.UserId)
            .ToListAsync(cancellationToken);

    private async Task<ConversationPeerDto> PeerAsync(
        Guid conversationId,
        Guid excludeUserId,
        CancellationToken cancellationToken)
    {
        var peer = await _dbContext.ConversationMembers
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId && m.UserId != excludeUserId)
            .Select(m => new PeerRow(
                m.UserId,
                m.User.UserName,
                m.User.DisplayName,
                m.User.ProfileImageUrl))
            .FirstOrDefaultAsync(cancellationToken);

        if (peer is null) return new ConversationPeerDto();

        return new ConversationPeerDto
        {
            Id = peer.UserId,
            UserName = peer.UserName,
            DisplayName = peer.DisplayName,
            ProfileImageUrl = peer.ProfileImageUrl,
        };
    }

    /// <summary>
    /// Hasta cuándo leyó cada interlocutor, por conversación. Se usa como
    /// corte para decidir si el último mensaje ya fue visto.
    /// </summary>
    private static Dictionary<Guid, DateTime> PeerReadCutoffs(
        List<ConversationMemberRow> members,
        Guid userId) =>
        members
            .Where(m => m.UserId != userId && m.LastReadAt.HasValue)
            .GroupBy(m => m.ConversationId)
            .ToDictionary(g => g.Key, g => g.Max(m => m.LastReadAt!.Value));

    /// <summary>
    /// Último mensaje de cada conversación. No se usa Max() sobre el Guid
    /// porque Postgres no tiene max(uuid): se ordena por fecha y se toma
    /// el primero, que EF traduce a un LIMIT 1 por grupo.
    /// </summary>
    private async Task<Dictionary<Guid, MessageDto>> LatestMessagesAsync(
        List<Guid> conversationIds,
        Dictionary<Guid, DateTime> seenCutoffs,
        CancellationToken cancellationToken)
    {
        var lastIds = await _dbContext.Messages
            .AsNoTracking()
            .Where(m => conversationIds.Contains(m.ConversationId))
            .GroupBy(m => m.ConversationId)
            .Select(g => new LastMessageStamp(g.Key, g.OrderByDescending(m => m.CreatedAt).First().Id))
            .ToListAsync(cancellationToken);

        if (lastIds.Count == 0) return [];

        var rows = await _dbContext.Messages
            .AsNoTracking()
            .Where(m => lastIds.Select(x => x.LastMessageId).Contains(m.Id))
            .Select(m => new MessageRow(
                m.Id,
                m.ConversationId,
                m.Content,
                m.CreatedAt,
                m.SenderId,
                m.Sender.UserName,
                m.Sender.DisplayName,
                m.Sender.ProfileImageUrl,
                m.EditedAt,
                m.IsDeleted))
            .ToListAsync(cancellationToken);

        // Cada conversación usa su propio corte, que es lo que leyó el otro.
        return rows.ToDictionary(
            r => r.ConversationId,
            r => MapMessage(r, seenCutoffs.GetValueOrDefault(r.ConversationId, DateTime.MinValue)));
    }
}
