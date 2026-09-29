using System.IdentityModel.Tokens.Jwt;
using Conectando.Api.Extensions;
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

    /// <summary>
    /// Identidad del usuario conectado, tomado del token.
    /// </summary>
    /// <remarks>
    /// Delega en la extensión de <c>ClaimsPrincipal</c> en vez de repetir la
    /// búsqueda. Antes lo hacía acá con <c>FindFirst(NameIdentifier)!</c> y sin
    /// alternativas: el token trae los claims cortos (<c>nameid</c>, <c>sub</c>)
    /// y, si no están mapeados al esquema de ASP.NET, esa búsqueda no encuentra
    /// nada y revienta. Como el hub corta la conexión si <c>OnConnectedAsync</c>
    /// falla, el síntoma era un chat que nunca recibía nada en vivo: el token
    /// luz la sesión entera: la app sigue igual, con el token vencido.
    /// Como el hub corta la conexión cuando <c>OnConnectedAsync</c> falla, el
    /// síntoma era un chat que no recibía nada en vivo y sin dar error.
    /// </remarks>
    protected Guid GetUserId() => Context.User!.GetUserId();

    /// <summary>Nombre visible del usuario conectado, o null si el token no lo trae.</summary>
    protected string? GetDisplayName() =>
        Context.User?.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value
        ?? Context.User?.Identity?.Name;

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
