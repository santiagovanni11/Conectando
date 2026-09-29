using Conectando.Api.Hubs.Broadcasting;
using Conectando.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Conectando.Api.Hubs;

/// <summary>
/// Canal en vivo de mensajes. Complementa al REST: el REST carga el
/// historial y este hub avisa apenas pasa algo.
/// </summary>
/// <remarks>
/// La pertenencia a grupos y el manejo de fallos vienen de
/// <see cref="ConectandoHub"/>; las reglas de difusion, de
/// <see cref="ConversationBroadcaster"/>. Aca solo quedan las acciones que el
/// cliente puede pedir en vivo.
/// </remarks>
// Se repite aqui a proposito: [Authorize] NO se hereda de la clase base,
// asi que solo ponerlo en ConectandoHub dejaria este hub abierto.

[Authorize]
public class MessageHub(
    IConversationService conversationService,
    ConversationBroadcaster broadcaster,
    ILogger<MessageHub> logger) : ConectandoHub(logger)
{
    private readonly IConversationService _conversationService = conversationService;
    private readonly ConversationBroadcaster _broadcaster = broadcaster;

    /// <summary>Deja de mandar eventos de una conversacion al salir de la vista.</summary>
    public Task LeaveConversation(Guid conversationId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, ConversationGroups.For(conversationId));

    /// <summary>Se suma al grupo de la conversacion abierta para recibir en vivo.</summary>
    public async Task JoinConversation(Guid conversationId)
    {
        // Antes de sumar al grupo se valida la pertenencia: si no, cualquiera
        // podria entrar en el grupo de un chat ajeno y enterarse de que existe.
        await _conversationService.GetMessagesAsync(GetUserId(), conversationId, null, 1);

        await Groups.AddToGroupAsync(Context.ConnectionId, ConversationGroups.For(conversationId));
        await Clients.Caller.SendAsync("JoinedConversation", new { ConversationId = conversationId });
    }

    /// <summary>Envia un mensaje y lo reenvia a los conectados en vivo.</summary>
    public async Task SendMessage(Guid conversationId, string content)
    {
        var message = await _conversationService.SendMessageAsync(GetUserId(), conversationId, content);
        await _broadcaster.MessageReceivedAsync(conversationId, message);
    }

    /// <summary>
    /// Edita un mensaje y avisa a la conversacion.
    /// </summary>
    /// <remarks>
    /// El cambio llega a los dos lados, incluido quien edita: asi su vista y
    /// la del otro quedan iguales sin recargar. Si el servidor rechaza
    /// -pasada la ventana de 15 minutos- el error sube a quien pidio la
    /// edicion, que es lo unico que la UI necesita para no perder el texto.
    /// </remarks>
    public async Task EditMessage(Guid conversationId, Guid messageId, string content)
    {
        var message = await _conversationService.EditMessageAsync(GetUserId(), conversationId, messageId, content);
        await _broadcaster.MessageChangedAsync(conversationId, message, "MessageUpdated");
    }

    /// <summary>Borra un mensaje y avisa a la conversacion, como en WhatsApp.</summary>
    public async Task DeleteMessage(Guid conversationId, Guid messageId)
    {
        var message = await _conversationService.DeleteMessageAsync(GetUserId(), conversationId, messageId);
        await _broadcaster.MessageChangedAsync(conversationId, message, "MessageDeleted");
    }

    /// <summary>
    /// Marca la conversacion como leida y avisa en vivo.
    /// </summary>
    /// <remarks>
    /// Se difunde a dos lados a proposito: al grupo de la conversacion
    /// (quien este mirando el hilo) y al grupo del interlocutor (quien este en
    /// la lista). Asi el "visto" aparece sin tener que abrir el chat.
    /// </remarks>
    public async Task MarkAsRead(Guid conversationId)
    {
        var userId = GetUserId();
        await _conversationService.MarkAsReadAsync(userId, conversationId);

        var peerIds = await _conversationService.GetPeerIdsAsync(conversationId, userId);

        await _broadcaster.ToConversationAndPeersAsync(conversationId, "MessageSeen", new
        {
            ConversationId = conversationId,
            ReaderId = userId,
            ReadAt = DateTime.UtcNow,
        }, peerIds);
    }

    /// <summary>
    /// Avisa que el usuario esta escribiendo. Se manda al grupo de la
    /// conversacion salvo a quien escribe, que ya sabe que esta escribiendo.
    /// </summary>
    public async Task Typing(Guid conversationId)
    {
        var userId = GetUserId();
        await _conversationService.EnsureIsMemberAsync(userId, conversationId);

        await Clients.OthersInGroup(ConversationGroups.For(conversationId))
            .SendAsync("UserTyping", new
            {
                ConversationId = conversationId,
                UserId = userId,
                DisplayName = Context.User?.FindFirst("unique_name")?.Value,
            });
    }
}
