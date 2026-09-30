using Conectando.Api.DTOs.Account;
using Conectando.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public partial class AccountService
{
    /// <summary>
    /// Cambia la contraseña y cierra las sesiones abiertas.
    /// </summary>
    /// <remarks>
    /// El orden importa: primero se valida la actual, después se confirma que
    /// la nueva está bien escrita, y recién al final se toca la base. Así un
    /// formulario con errores no deja la cuenta a medio cambiar.
    /// </remarks>
    public async Task ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await RequireActiveUserAsync(userId, cancellationToken);

        RequireCurrentPassword(user, request.CurrentPassword);

        if (request.NewPassword != request.ConfirmPassword)
        {
            throw new PasswordConfirmationMismatchException();
        }

        if (PasswordPolicy.Validate(request.NewPassword) is { } reason)
        {
            throw new WeakPasswordException(reason);
        }

        if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
        {
            throw new SamePasswordException();
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        // Rota el sello: los tokens ya emitidos dejan de servir, y cambiar la
        // contraseña también cierra las sesiones abiertas en otros lados.
        user.SecurityStamp = Guid.NewGuid();
        user.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }
}
