using System.Security.Claims;
using Conectando.Api.Data;
using Conectando.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Extensions;

/// <summary>
/// Revisa, en cada request, que el token siga sirviendo.
/// </summary>
/// <remarks>
/// El token se valida por firma y por fecha, sin mirar la base. Eso alcanza
/// para saber que nadie lo alteró, pero no alcanza para saber si la cuenta
/// sigue existiendo: un token robado seguiría sirviendo hasta vencerse,
/// aunque el dueño hubiera cambiado la contraseña. Por eso se compara el
/// sello que viaja en el token con el que tiene la cuenta ahora.
/// </remarks>
public static class AccountTokenValidation
{
    public static async Task RejectStaleSessionsAsync(TokenValidatedContext context)
    {
        var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(id, out var userId))
        {
            return;
        }

        var tokenStamp = context.Principal?.FindFirstValue(JwtService.SecurityStampClaim);

        var db = context.HttpContext.RequestServices.GetRequiredService<ConectandoDbContext>();
        var account = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.SecurityStamp, u.DeletedAt })
            .SingleOrDefaultAsync(context.HttpContext.RequestAborted);

        // Tres motivos para rechazar y el mismo final: la cuenta ya no existe,
        // fue dada de baja, o el sello del token no es el vigente.
        if (account is null
            || account.DeletedAt is not null
            || tokenStamp != account.SecurityStamp.ToString())
        {
            context.Fail("La sesión ya no es válida.");
        }
    }
}
