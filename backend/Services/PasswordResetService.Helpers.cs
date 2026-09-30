using Conectando.Api.Data;
using Conectando.Api.Models;
using Conectando.Api.Settings;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Utilidades de la recuperación de contraseña.
/// </summary>
public partial class PasswordResetService
{
    /// <summary>
    /// La cuenta activa con ese correo, o null.
    ///
    /// Se filtra por DeletedAt: una cuenta dada de baja no puede pedir
    /// códigos. Y se compara en minúsculas porque el correo no distingue
    /// mayúsculas y "Ana@x.com" y "ana@x.com" son la misma casilla: si se
    /// trataran distinto, el usuario escribiría su correo y no le llegaría
    /// nada, sin explicación.
    /// </summary>
    private Task<AppUser?> FindActiveUserAsync(string email, CancellationToken cancellationToken) =>
        _db.Users.FirstOrDefaultAsync(
            u => u.DeletedAt == null && u.Email.ToLower() == email.Trim().ToLower(),
            cancellationToken);

    /// <summary>Si ya pidió un código hace poco, no se genera otro.</summary>
    private Task<bool> HayPeticionRecienteAsync(Guid userId, CancellationToken cancellationToken) =>
        _db.PasswordResetCodes
            .AnyAsync(
                c => c.UserId == userId
                    && c.CreatedAt > DatabaseTime.UtcNow() - PasswordResetPolicy.ResendCooldown,
                cancellationToken);

    /// <summary>
    /// Código de N dígitos al azar.
    ///
    /// Con <see cref="RandomNumberGenerator"/> y no con <c>System.Random</c>:
    /// este último es predecible a partir de pocos valores iniciales, y
    /// quien lo conociera podría calcular los códigos de los demás.
    /// </summary>
    private static string GenerarCodigo()
    {
        var max = (int)Math.Pow(10, PasswordResetPolicy.CodeDigits);

        return System.Security.Cryptography.RandomNumberGenerator
            .GetInt32(0, max)
            .ToString()
            .PadLeft(PasswordResetPolicy.CodeDigits, '0');
    }

    /// <summary>
    /// Borra los códigos vencidos de hace más de un día.
    ///
    /// Se conservan un rato después de vencer para que la espera entre pedir
    /// el código y mandarlo se mida contra algo real, y no contra un vacío.
    /// </summary>
    private async Task LimpiarVencidosAsync(CancellationToken cancellationToken)
    {
        var corte = DatabaseTime.UtcNow() - PasswordResetPolicy.HistoryRetention;

        var viejos = await _db.PasswordResetCodes
            .Where(c => c.ExpiresAt < corte)
            .ToListAsync(cancellationToken);

        if (viejos.Count == 0) return;

        _db.PasswordResetCodes.RemoveRange(viejos);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
