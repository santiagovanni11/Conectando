using Conectando.Api.Data;
using Conectando.Api.DTOs.Messages;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Conectando.Api.Settings;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Conversaciones y mensajes. Se parte en partial class para que
/// ningún archivo crezca sin límite.
/// </summary>
public partial class ConversationService(
    ConectandoDbContext dbContext,
    IBlockService blockService) : IConversationService
{
    private const int MaxLimit = 100;
    private readonly ConectandoDbContext _dbContext = dbContext;
    private readonly IBlockService _blockService = blockService;

    /// <summary>
    /// Elimina un chat solo para quien lo pide, como en WhatsApp. La otra
    /// persona lo sigue teniendo; si vuelve a escribir, el chat reaparece
    /// para quien lo había borrado.
    /// </summary>
    public async Task DeleteConversationAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var member = await _dbContext.ConversationMembers
            .FirstOrDefaultAsync(
                m => m.ConversationId == conversationId && m.UserId == userId,
                cancellationToken);

        if (member is null) throw new ConversationNotFoundException();

        member.DeletedAt = DatabaseTime.UtcNow();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Silencia o reactiva un chat. No borra nada: el otro sigue escribiendo
    /// y los mensajes se siguen viendo, solo deja de llegar la notificación.
    /// </summary>
    public async Task<bool> SetMutedAsync(
        Guid userId,
        Guid conversationId,
        bool muted,
        CancellationToken cancellationToken = default)
    {
        var member = await _dbContext.ConversationMembers
            .FirstOrDefaultAsync(
                m => m.ConversationId == conversationId && m.UserId == userId,
                cancellationToken);

        if (member is null) throw new ConversationNotFoundException();

        member.MutedAt = muted ? DatabaseTime.UtcNow() : null;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return muted;
    }

    /// <summary>El chat vuelve a aparecer si la otra persona escribe algo nuevo.</summary>
    public async Task<MessageDto> SendMessageAsync(
        Guid userId,
        Guid conversationId,
        string content,
        CancellationToken cancellationToken = default)
    {
        var trimmed = content?.Trim() ?? string.Empty;
        if (trimmed.Length == 0) throw new EmptyMessageException();
        if (trimmed.Length > MessageLimits.MaxLength) throw new MessageTooLongException();

        await EnsureIsMemberAsync(userId, conversationId, cancellationToken);
        await EnsureCanWriteAsync(userId, conversationId, cancellationToken);

        // Si el destinatario había borrado el chat, un mensaje nuevo lo
        // resurrecta: es el comportamiento de WhatsApp.
        var membership = await _dbContext.ConversationMembers
            .FirstOrDefaultAsync(
                m => m.ConversationId == conversationId && m.UserId != userId,
                cancellationToken);

        if (membership is not null && membership.DeletedAt is not null)
        {
            membership.DeletedAt = null;
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = userId,
            Content = trimmed,
            CreatedAt = DatabaseTime.UtcNow(),
        };

        _dbContext.Messages.Add(message);

        var conversation = await _dbContext.Conversations.FirstAsync(
            c => c.Id == conversationId, cancellationToken);
        conversation.UpdatedAt = message.CreatedAt;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await BuildMessageDtoAsync(message.Id, cancellationToken);
    }

    public async Task MarkAsReadAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var member = await _dbContext.ConversationMembers
            .FirstOrDefaultAsync(
                m => m.ConversationId == conversationId && m.UserId == userId,
                cancellationToken);

        if (member is null) throw new ConversationNotFoundException();

        member.LastReadAt = DatabaseTime.UtcNow();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>No se puede escribir en una conversación con alguien que te bloqueó.</summary>
    private async Task EnsureCanWriteAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var peerIds = await PeerIdsAsync(conversationId, userId, cancellationToken);

        // Bloqueo entre pares: IsBlockedAsync ya es simétrico.
        foreach (var peerId in peerIds)
        {
            if (await _blockService.IsBlockedAsync(userId, peerId, cancellationToken))
            {
                throw new BlockedActionException();
            }
        }
    }
}
