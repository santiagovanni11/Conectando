using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Conectando.Api.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Conectando.Api.Services;

public class JwtService(IOptions<JwtSettings> options) : IJwtService
{
    /// <summary>
    /// Claim con el sello de la cuenta. Viaja en el token para que la
    /// validación pueda compararlo con el de la base y cerrar sesiones viejas.
    /// </summary>
    public const string SecurityStampClaim = "security_stamp";

    private readonly JwtSettings _settings = options.Value;

    public string CreateToken(AppUser user, out DateTime expiresAt)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(SecurityStampClaim, user.SecurityStamp.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        expiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}