using Conectando.Api.Exceptions;
using Conectando.Api.Settings;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Canje del código por una contraseña nueva.
/// </summary>
public partial class PasswordResetService
{
    /// <summary>
    /// Comprueba el código y cambia la contraseña.
    ///
    /// Todos los caminos de falla devuelven el mismo error, esté mal el
    /// código, haya vencido, se agotaron los intentos o el correo no exista.
    /// Distinguirlos le diría a un atacante cuántas combinaciones le quedan
    /// por probar, y hasta le confirmaría si una casilla está registrada.
    /// </summary>
    public async Task ConfirmAsync(
        DTOs.Account.ConfirmPasswordResetRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await FindActiveUserAsync(request.Email, cancellationToken);
        if (user is null) throw new InvalidResetCodeException();

        var code = await VigenteAsync(user.Id, cancellationToken);
        if (code is null) throw new InvalidResetCodeException();

        if (!BCrypt.Net.BCrypt.Verify(request.Code.Trim(), code.CodeHash))
        {
            // Se cuenta el intento y se guarda. Sin esto, el tope de intentos
            // no existiría: se podría probar indefinidamente contra el mismo
            // código hasta acertar.
            code.Attempts++;
            await _db.SaveChangesAsync(cancellationToken);

            if (code.Attempts >= PasswordResetPolicy.MaxAttempts)
            {
                // Se quema: se invalida sin decir por qué.
                code.UsedAt = DatabaseTime.UtcNow();
                await _db.SaveChangesAsync(cancellationToken);
            }

            throw new InvalidResetCodeException();
        }

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

        // Rota el sello y con eso mueren los tokens ya emitidos. Es lo que
        // cierra las sesiones abiertas: si alguien tenía la sesión
        // iniciada en otro dispositivo, deja de servir en el momento del
        // cambio, no cuando caduque el token.
        user.SecurityStamp = Guid.NewGuid();
        user.UpdatedAt = DatabaseTime.UtcNow();
        code.UsedAt = DatabaseTime.UtcNow();

        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// El código vigente del usuario: el más reciente que no se usó, no
    /// venció y todavía le quedan intentos.
    /// </summary>
    private Task<Models.PasswordResetCode?> VigenteAsync(Guid userId, CancellationToken cancellationToken) =>
        _db.PasswordResetCodes
            .Where(c => c.UserId == userId
                && c.UsedAt == null
                && c.ExpiresAt > DatabaseTime.UtcNow()
                && c.Attempts < PasswordResetPolicy.MaxAttempts)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
}
