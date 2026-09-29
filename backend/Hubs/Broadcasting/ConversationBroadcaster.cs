using Conectando.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Conectando.Api.Hubs.Broadcasting;

/// <summary>
/// Reglas de difusion de los eventos de una conversacion.
/// </summary>
/// <remarks>
/// Va aparte del hub porque la difusion no es "un metodo mas": es la misma
/// regla para los cuatro eventos (llego, se edito, se borro, se vio). Al
/// repetirla dentro del hub, ese archivo paso las 150 lineas. Aca queda una
/// sola definicion de a quien le llega cada cosa.
/// </remarks>
public class ConversationBroadcaster(
    IHubContext<MessageHub> hub,
    ILogger<ConversationBroadcaster> logger)
{
    private readonly IHubContext<MessageHub> _hub = hub;
    private readonly ILogger<ConversationBroadcaster> _logger = logger;

    /// <summary>
    /// Entrega un aviso sin que su fallo se confunda con un fallo de la escritura.
    /// </summary>
    /// <remarks>
    /// Cuando se avisa, la operacion ya quedo guardada. Si la entrega fallara
    /// y el error subiera, el front creeria que no se guardo, caeria al REST
    /// y guardaria dos veces. Perder un aviso en vivo se arregla recargando;
    /// duplicar una escritura, no. Por eso todo lo de abajo delivera con
    /// Notify en lugar de dejar que el error suba.
    /// </remarks>
    private async Task Notify(Func<Task> send)
    {
        try
        {
            await send();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "No se pudo difundir el evento en vivo");
        }
    }

    private Task SendToConversation(Guid conversationId, string eventName, object payload) =>
        _hub.Clients.Group(ConversationGroups.For(conversationId)).SendAsync(eventName, payload);

    /// <summary>
    /// Mensaje nuevo. Se difunde al grupo entero y cada cliente descarta
    /// el suyo por <c>SenderId</c>.
    /// </summary>
    /// <remarks>
    /// No se usa <c>GroupExcept</c> porque solo el emisor conoce su
    /// connectionId: excluir al receptor exigiria conocer el suyo, que el
    /// servidor no tiene. Es lo mismo que ya hace <c>MarkAsRead</c> con el
    /// <c>ReaderId</c>, asi que el frontend ya sabe filtrar por remitente.
    /// </remarks>
    public Task MessageReceivedAsync(Guid conversationId, object message) =>
        Notify(() => SendToConversation(conversationId, "MessageReceived", message));

    /// <summary>
    /// Edicion o borrado. Le llega a los dos lados, incluido quien lo hizo:
    /// su propia vista se actualiza con la respuesta del servidor y ambos
    /// tienen que ver lo mismo.
    /// </summary>
    public Task MessageChangedAsync(Guid conversationId, object message, string eventName) =>
        Notify(() => SendToConversation(conversationId, eventName, message));

    /// <summary>
    /// Difunde a la conversacion abierta y a los interlocutores. El doble
    /// destino es lo que hace que el "visto" se vea sin abrir el chat.
    /// </summary>
    public async Task ToConversationAndPeersAsync(
        Guid conversationId,
        string eventName,
        object payload,
        IReadOnlyList<Guid> peerIds)
    {
        await Notify(() => SendToConversation(conversationId, eventName, payload));

        // Uno por uno y sin cortar el resto: que un interlocutor este caido
        // no puede deprive al otro de enterarse.
        foreach (var peerId in peerIds)
        {
            await Notify(() => _hub.Clients.Group(UserGroups.For(peerId)).SendAsync(eventName, payload));
        }
    }
}