using Conectando.Api.Models;

namespace Conectando.Api.Interfaces;

public interface IJwtService
{
    string CreateToken(AppUser user, out DateTime expiresAt);
}