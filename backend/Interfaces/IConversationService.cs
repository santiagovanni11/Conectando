using Conectando.Api.DTOs.Messages;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Settings;

namespace Conectando.Api.Interfaces;

public interface IConversationService
{
    Task<List<ConversationDto>> GetConversationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ConversationDto> StartDirectAsync(Guid userId, Guid recipientId, CancellationToken cancellationToken = default);
    Task<MessagePageDto> GetMessagesAsync(Guid userId, Guid conversationId, string? cursor, int limit, CancellationToken cancellationToken = default);
    Task<MessageDto> SendMessageAsync(Guid userId, Guid conversationId, string content, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirma que el usuario pertenece a la conversación. Lo usan el hub
    /// para no difundir eventos a conversations ajenas.
    /// </summary>
    Task EnsureIsMemberAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Borra el chat solo para este usuario. Los demás lo siguen teniendo.
    /// </summary>
    Task DeleteConversationAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default);

    /// <summary>Silencia o reactiva un chat. Devuelve el estado resultante.</summary>
    Task<bool> SetMutedAsync(Guid userId, Guid conversationId, bool muted, CancellationToken cancellationToken = default);

    Task<MessageDto> EditMessageAsync(
        Guid userId, Guid conversationId, Guid messageId, string content,
        CancellationToken cancellationToken = default);

    Task<MessageDto> DeleteMessageAsync(
        Guid userId, Guid conversationId, Guid messageId,
        CancellationToken cancellationToken = default);

    /// <summary>Los otros participantes de la conversación, sin contar uno mismo.</summary>
    Task<List<Guid>> GetPeerIdsAsync(Guid conversationId, Guid excludeUserId, CancellationToken cancellationToken = default);
}