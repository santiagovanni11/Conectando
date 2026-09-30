using Conectando.Api.Data;
using Conectando.Api.DTOs.Messages;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public partial class ConversationService
{
    public async Task<ConversationDto> StartDirectAsync(
        Guid userId,
        Guid recipientId,
        CancellationToken cancellationToken = default)
    {
        if (userId == recipientId) throw new SelfActionException();

        var recipientExists = await _dbContext.Users
            .AnyAsync(u => u.Id == recipientId, cancellationToken);

        if (!recipientExists) throw new UserNotFoundException();

        // Una conversación directa es la que tiene a ambos como miembros.
        var existingId = await _dbContext.ConversationMembers
            .Where(m => m.UserId == userId || m.UserId == recipientId)
            .GroupBy(m => m.ConversationId)
            .Where(g => g.Count() == 2)
            .Select(g => (Guid?)g.Key)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingId is not null)
        {
            var current = await _dbContext.Conversations
                .AsNoTracking()
                .SingleAsync(c => c.Id == existingId.Value, cancellationToken);

            return new ConversationDto
            {
                Id = current.Id,
                UpdatedAt = current.UpdatedAt,
                Peers = [await PeerAsync(existingId.Value, userId, cancellationToken)],
            };
        }

        var now = DatabaseTime.UtcNow();
        var conversation = new Conversation { Id = Guid.NewGuid(), CreatedAt = now, UpdatedAt = now };

        _dbContext.Conversations.Add(conversation);
        _dbContext.ConversationMembers.AddRange(
            new ConversationMember { ConversationId = conversation.Id, UserId = userId, JoinedAt = now },
            new ConversationMember { ConversationId = conversation.Id, UserId = recipientId, JoinedAt = now });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ConversationDto
        {
            Id = conversation.Id,
            UpdatedAt = conversation.UpdatedAt,
            Peers = [await PeerAsync(conversation.Id, userId, cancellationToken)],
        };
    }

    private static Dictionary<Guid, int> UnreadCountsFrom(
        List<PeerMessageStamp> messages,
        Dictionary<Guid, DateTime?> lastRead,
        Guid userId)
    {
        return messages
            .GroupBy(m => m.ConversationId)
            .ToDictionary(
                g => g.Key,
                g => g.Count(m => m.CreatedAt > (lastRead.GetValueOrDefault(g.Key) ?? DateTime.MinValue)));
    }

    /// <summary>
    /// Confirma la pertenencia. Es pública porque el hub la usa para no
    /// difundir eventos de una conversación a quien no participa.
    /// </summary>
    public async Task EnsureIsMemberAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default)
    {
        var isMember = await _dbContext.ConversationMembers
            .AnyAsync(
                m => m.ConversationId == conversationId && m.UserId == userId,
                cancellationToken);

        if (!isMember) throw new ConversationNotFoundException();
    }

    /// <summary>
    /// IDs de los demás participantes. Lo usa el hub para avisarles
    /// cuando alguien leyó la conversación, aunque no tengan el chat abierto.
    /// </summary>
    public Task<List<Guid>> GetPeerIdsAsync(
        Guid conversationId,
        Guid excludeUserId,
        CancellationToken cancellationToken = default) => PeerIdsAsync(conversationId, excludeUserId, cancellationToken);
}
