using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Conectando.Api.Hubs;

/// <summary>
/// Nombres de los grupos de SignalR. Se agrupan por entidad: uno por
/// usuario (para avisarle a un lado) y uno por conversacion (para avisarle
/// a los dos que participan).
/// </summary>
internal static class UserGroups
{
    public static string For(Guid userId) => $"user:{userId}";
}

internal static class ConversationGroups
{
    public static string For(Guid conversationId) => $"conversation:{conversationId}";
}

/// <summary>
/// Base de los hubs: identidad de quien llama y forma de reportar un fallo.
/// </summary>
/// <remarks>
/// Todo hub necesita las tres cosas. Dejarlas aca evita que cada uno las
/// vuelva a escribir y que el manejo de errores se disperse por el proyecto.
/// </remarks>
[Authorize]
public abstract class ConectandoHub(ILogger logger) : Hub
{
    private readonly ILogger _logger = logger;

    /// <summary>Identidad del usuario conectado, tomado del token.</summary>
    protected Guid GetUserId() =>
        Guid.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>
    /// Entra al grupo del usuario al conectarse, para que los avisos que van
    /// solo a una persona (el "visto", por ejemplo) le lleguen sin importar
    /// que chat este abierto.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, UserGroups.For(userId));

        await Clients.Caller.SendAsync("Connected", new { UserId = userId });
        await base.OnConnectedAsync();
    }
}
