using Conectando.Api.DTOs.Messages;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Settings;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Envío de mensajes, con o sin cita de otro mensaje.
/// </summary>
public partial class ConversationService
{
    /// <summary>El chat vuelve a aparecer si la otra persona escribe algo nuevo.</summary>
    public async Task<MessageDto> SendMessageAsync(
        Guid userId,
        Guid conversationId,
        string content,
        Guid? replyToMessageId = null,
        CancellationToken cancellationToken = default)
    {
        var trimmed = content?.Trim() ?? string.Empty;
        if (trimmed.Length == 0) throw new EmptyMessageException();
        if (trimmed.Length > MessageLimits.MaxLength) throw new MessageTooLongException();

        await EnsureIsMemberAsync(userId, conversationId, cancellationToken);
        await EnsureCanWriteAsync(userId, conversationId, cancellationToken);
        if (replyToMessageId is not null)
        {
            await EnsureIsSameConversationAsync(
                conversationId, replyToMessageId.Value, cancellationToken);
        }

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
            ReplyToMessageId = replyToMessageId,
        };

        _dbContext.Messages.Add(message);

        var conversation = await _dbContext.Conversations.FirstAsync(
            c => c.Id == conversationId, cancellationToken);
        conversation.UpdatedAt = message.CreatedAt;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await BuildMessageDtoAsync(message.Id, cancellationToken);
    }

    /// <summary>
    /// Comprueba que el mensaje citado pertenezca a esta conversación.
    ///
    /// Es un control de seguridad, no una validación de forma. La cita viaja
    /// en la respuesta de la API con el texto del original: si aceptáramos
    /// cualquier id, quien mandara el POST podría citar un mensaje de otro
    /// chat y leer su contenido sin ser parte de esa conversación. La
    /// pertenencia al hilo se comprueba siempre, contra la misma
    /// conversación del envío, no contra la del mensaje citado.
    /// </summary>
    private async Task EnsureIsSameConversationAsync(
        Guid conversationId,
        Guid replyToMessageId,
        CancellationToken cancellationToken)
    {
        var belongs = await _dbContext.Messages
            .AsNoTracking()
            .AnyAsync(
                m => m.Id == replyToMessageId && m.ConversationId == conversationId,
                cancellationToken);

        if (!belongs) throw new MessageNotFoundException();
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
