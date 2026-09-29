using Conectando.Api.Data;
using Conectando.Api.DTOs.Messages;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public partial class ConversationService
{
    public async Task<List<ConversationDto>> GetConversationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var conversationIds = await _dbContext.ConversationMembers
            .Where(m => m.UserId == userId && m.DeletedAt == null)
            .OrderByDescending(m => m.Conversation!.UpdatedAt)
            .Select(m => m.ConversationId)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (conversationIds.Count == 0) return [];

        var rows = await _dbContext.Conversations
            .AsNoTracking()
            .Where(c => conversationIds.Contains(c.Id))
            .Select(c => new { c.Id, c.UpdatedAt })
            .ToListAsync(cancellationToken);

        var ids = rows.Select(r => r.Id).ToList();

        // Se trae aparte: anidar la subconsulta en la proyección rompe EF.
        var lastRead = await _dbContext.ConversationMembers
            .AsNoTracking()
            .Where(m => m.UserId == userId && ids.Contains(m.ConversationId))
            .Select(m => new { m.ConversationId, m.LastReadAt })
            .ToDictionaryAsync(x => x.ConversationId, x => x.LastReadAt, cancellationToken);

        // Se proyecta a un record propio: el operador ! de nulidad dentro
        // de una proyección anónima hizo que EF no resolviera los campos.
        var members = await _dbContext.ConversationMembers
            .AsNoTracking()
            .Where(m => ids.Contains(m.ConversationId))
            .Select(m => new ConversationMemberRow(
                m.ConversationId,
                m.UserId,
                m.User.UserName,
                m.User.DisplayName,
                m.User.ProfileImageUrl,
                m.LastReadAt))
            .ToListAsync(cancellationToken);

        // La previsualización también lleva su "visto": el corte es lo que
        // leyó el otro, para que el tick se vea sin abrir el chat.
        var seenCutoffs = PeerReadCutoffs(members, userId);
        var lastMessages = await LatestMessagesAsync(ids, seenCutoffs, cancellationToken);

        // El silencio es por usuario: cada uno silencia el chat para sí.
        var muted = await _dbContext.ConversationMembers
            .AsNoTracking()
            .Where(m => m.UserId == userId && ids.Contains(m.ConversationId) && m.MutedAt != null)
            .Select(m => m.ConversationId)
            .ToDictionaryAsync(id => id, _ => true, cancellationToken);

        var allPeerMessages = await _dbContext.Messages
            .AsNoTracking()
            .Where(m => ids.Contains(m.ConversationId) && m.SenderId != userId)
            .Select(m => new PeerMessageStamp(m.ConversationId, m.CreatedAt))
            .ToListAsync(cancellationToken);

        var unread = UnreadCountsFrom(allPeerMessages, lastRead, userId);

        return rows
            .OrderByDescending(r => r.UpdatedAt)
            .Select(r => new ConversationDto
            {
                Id = r.Id,
                UpdatedAt = r.UpdatedAt,
                UnreadCount = unread.GetValueOrDefault(r.Id),
                Peers = members
                    .Where(m => m.ConversationId == r.Id && m.UserId != userId)
                    .Select(m => new ConversationPeerDto
                    {
                        Id = m.UserId,
                        UserName = m.UserName,
                        DisplayName = m.DisplayName,
                        ProfileImageUrl = m.ProfileImageUrl,
                    })
                    .ToList(),
                LastMessage = lastMessages.GetValueOrDefault(r.Id),
                IsMuted = muted.ContainsKey(r.Id),
            })
            .ToList();
    }

}
