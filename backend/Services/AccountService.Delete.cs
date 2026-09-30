using Conectando.Api.DTOs.Account;
using Conectando.Api.Exceptions;

namespace Conectando.Api.Services;

public partial class AccountService
{
    /// <summary>
    /// Da de baja la cuenta: borra todo lo que dejó y deja la fila vacía.
    /// </summary>
    /// <remarks>
    /// La fila no se borra. Los mensajes la apuntan con clave foránea y el chat
    /// del otro tiene que seguir ahí: si la fila desapareciera, o el mensaje
    /// se iría por cascada o la base lo impediría. Entonces se limpia todo lo
    /// demás y la fila queda como un cascarón, con los datos personales
    /// sustituidos.
    ///
    /// <para>
    /// Lo que ve el otro en ese chat es "cuenta eliminada", no el cascarón:
    /// ver un identificador técnico en pantalla confirma que existió una
    /// cuenta y parece un error, cuando en realidad es el resultado esperado.
    /// </para>
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

        await PurgarAsync(userId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Deja la cuenta sin rastro personal y sin posibilidad de entrar.
    /// </summary>
    /// <remarks>
    /// El email y el nombre de usuario llevan el id para seguir siendo únicos:
    /// los usa la base como clave, y si dos cuentas eliminadas compartieran
    /// "eliminado", la segunda no se podría guardar. Ese id nunca se muestra —
    /// <c>UserPresentation</c> lo sustituye por una etiqueta neutra antes de
    /// que llegue a la pantalla— pero hace falta que sea único en la base.
    ///
    /// <para>
    /// La contraseña se reemplaza por un hash aleatorio en vez de vaciarse,
    /// porque un hash vacío rompería la verificación en vez de rechazarla.
    /// </para>
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
