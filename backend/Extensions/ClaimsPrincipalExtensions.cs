using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Conectando.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (value is null || !Guid.TryParse(value, out var userId))
        {
            throw new ArgumentException("El token no contiene un identificador de usuario válido.", nameof(principal));
        }

        return userId;
    }
}