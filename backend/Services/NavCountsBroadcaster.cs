using Conectando.Api.Hubs;
using Conectando.Api.Hubs.Broadcasting;
using Conectando.Api.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Conectando.Api.Services;

/// <summary>
/// Manda los contadores de la navegación al usuario que corresponde.
/// </summary>
/// <remarks>
/// El evento se manda al grupo del usuario, no al de la conversación: cada
/// quien está conectado se suma a su propio grupo al conectarse, así que no
/// hace falta saber qué conversaciones tiene abiertas. Un mensaje en un chat
/// que no estás mirando igual tiene que cambiar tu contador.
///
/// Los números viajan en el evento, calculados. Si en cambio se mandara solo
/// un "cambiaron", el cliente tendría que volver a preguntar, y ese viaje de
/// ida y vuelta es justo lo que hace que el número tarde en aparecer.
/// </remarks>
public class NavCountsBroadcaster(
    IHubContext<MessageHub> hub,
    INavCountService counts,
    ILogger<NavCountsBroadcaster> logger) : INavCountsBroadcaster
{
    public Task NotifyAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Send(userId, cancellationToken);

    public async Task NotifyManyAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        // Uno por uno: el destinatario de un mensaje se determina mirando si
        // está en la conversación, y en el camino puede haber varios.
        foreach (var userId in userIds.Distinct())
        {
            await Send(userId, cancellationToken);
        }
    }

    private async Task Send(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var data = await counts.GetCountsAsync(userId, cancellationToken);
            await hub.Clients
                .Group(UserGroups.For(userId))
                .SendAsync(NavCountsEvents.Changed, data, cancellationToken);
        }
        catch (Exception exception)
        {
            // El aviso va con lo ya guardado. Si fallara y el error subiera, el
            // usuario creería que su mensaje, su like o su solicitud no se
            // guardaron, y volvería a intentarlo. Perder un número en vivo se
            // arregla recargando; duplicar una escritura, no.
            logger.LogWarning(exception, "No se pudieron avisar los contadores a {UserId}", userId);
        }
    }
}
