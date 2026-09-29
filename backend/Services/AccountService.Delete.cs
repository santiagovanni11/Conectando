using Conectando.Api.DTOs.Account;
using Conectando.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public partial class AccountService
{
    /// <summary>
    /// Da de baja la cuenta anonimizándola.
    /// </summary>
    /// <remarks>
    /// No se borra la fila. Mensajes, comentarios, publicaciones y Likes la
    /// apuntan con clave foránea: borrarla de verdad revienta la base, y
    /// cambiar esas relaciones a cascada llevaría por delante el contenido de
    /// otras personas. Entonces se limpian los datos personales y se conserva
    /// el vínculo, para que el chat del otro siga ahí.
    /// </remarks>
    public async Task DeleteAccountAsync(
        Guid userId,
        DeleteAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await RequireActiveUserAsync(userId, cancellationToken);

        RequireCurrentPassword(user, request.CurrentPassword);

        if (request.CurrentPassword != request.ConfirmPassword)
        {
            throw new PasswordConfirmationMismatchException();
        }

        Anonymize(user);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Deja la cuenta sin rastro personal y sin posibilidad de entrar.
    /// </summary>
    /// <remarks>
    /// El email y el nombre de usuario llevan el id para seguir siendo únicos:
    /// los usa la base como clave, y si dos cuentas eliminadas compartieran
    /// "eliminado", la segunda no se podría guardar. La contraseña se
    /// reemplaza por un hash aleatorio en vez de vaciarse, porque un hash
    /// vacío rompería la verificación en vez de rechazarla.
    /// </remarks>
    private static void Anonymize(Models.AppUser user)
    {
        var now = DateTime.UtcNow;

        user.Email = $"eliminado-{user.Id:N}@conectando.invalid";
        user.UserName = $"eliminado-{user.Id:N}";
        user.DisplayName = "Usuario eliminado";
        user.Bio = null;
        user.ProfileImageUrl = null;
        user.IsPrivate = false;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());
        user.SecurityStamp = Guid.NewGuid();
        user.UpdatedAt = now;
        user.DeletedAt = now;
    }
}
