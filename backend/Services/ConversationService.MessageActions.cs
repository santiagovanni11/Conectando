using Conectando.Api.Data;
using Conectando.Api.DTOs.Messages;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Settings;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Edición y borrado de mensajes.
///
/// El borrado es lógico: se marca <c>IsDeleted</c> y el contenido deja de
/// enviarse. Es la única opción segura: si borráramos la fila, el hilo
/// quedaría con un hueco y no se podría probar quién lo borró.
///
/// La edición tiene ventana de tiempo, como en WhatsApp, para que la
/// conversación no se pueda reescribir a posteriori.
/// </summary>
public partial class ConversationService
{
    /// <summary>Minutos dentro de los cuales se puede editar un mensaje.</summary>
    private const int EditWindowMinutes = 15;

    public async Task<MessageDto> EditMessageAsync(
        Guid userId,
        Guid conversationId,
        Guid messageId,
        string content,
        CancellationToken cancellationToken = default)
    {
        var trimmed = content?.Trim() ?? string.Empty;
        if (trimmed.Length == 0) throw new EmptyMessageException();
        if (trimmed.Length > MessageLimits.MaxLength) throw new MessageTooLongException();

        await EnsureIsMemberAsync(userId, conversationId, cancellationToken);

        var message = await LoadOwnedMessageAsync(userId, conversationId, messageId, cancellationToken);

        if (message.IsDeleted) throw new MessageNotFoundException();

        var age = DatabaseTime.UtcNow() - message.CreatedAt;
        if (age > TimeSpan.FromMinutes(EditWindowMinutes))
        {
            throw new MessageEditExpiredException(EditWindowMinutes);
        }

        message.Content = trimmed;
        message.EditedAt = DatabaseTime.UtcNow();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await BuildMessageDtoAsync(message.Id, cancellationToken);
    }

    public async Task<MessageDto> DeleteMessageAsync(
        Guid userId,
        Guid conversationId,
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        await EnsureIsMemberAsync(userId, conversationId, cancellationToken);

        var message = await LoadOwnedMessageAsync(userId, conversationId, messageId, cancellationToken);

        if (message.IsDeleted) return await BuildMessageDtoAsync(message.Id, cancellationToken);

        message.IsDeleted = true;
        message.Content = string.Empty;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await BuildMessageDtoAsync(message.Id, cancellationToken);
    }

    /// <summary>
    /// Carga un mensaje comprobando que exista, pertenezca a la conversación
    /// y sea del usuario. Los tres chequeos juntos son lo que impide que
    /// alguien toque mensajes ajenos.
    /// </summary>
    private async Task<Message> LoadOwnedMessageAsync(
        Guid userId,
        Guid conversationId,
        Guid messageId,
        CancellationToken cancellationToken)
    {
        var message = await _dbContext.Messages
            .FirstOrDefaultAsync(
                m => m.Id == messageId && m.ConversationId == conversationId,
                cancellationToken);

        if (message is null) throw new MessageNotFoundException();
        if (message.SenderId != userId) throw new MessageOwnershipException();

        return message;
    }
}