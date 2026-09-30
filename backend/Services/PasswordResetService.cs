using Conectando.Api.Data;
using Conectando.Api.Interfaces;
using Conectando.Api.Settings;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Recuperación de contraseña por código.
/// </summary>
public partial class PasswordResetService : IPasswordResetService
{
    private readonly ConectandoDbContext _db;
    private readonly IEmailSender _email;

    /// <summary>Nombre con el que se firma el correo, desde la configuración.</summary>
    private readonly string _marca;

    public PasswordResetService(
        ConectandoDbContext db,
        IEmailSender email,
        MailSettings settings)
    {
        _db = db;
        _email = email;
        _marca = settings.BrandName;
    }

    /// <summary>
    /// Pide un código para un correo.
    ///
    /// Devuelve siempre el mismo resultado, exista o no la cuenta. Es la
    /// regla más importante de todo el flujo: si acá se dijera "ese correo no
    /// está registrado", cualquiera podría recorrer la base probando correos
    /// y armarse una lista de quién tiene cuenta, sin entrar nunca. El
    /// usuario recibe siempre el mismo cartel y no puede deducir nada.
    /// </summary>
    public async Task RequestCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await FindActiveUserAsync(email, cancellationToken);
        if (user is null) return;

        // Una sola espera para todo: sin esto, alguien pide códigos en cadena
        // para inundar el correo de otra persona y gastar la cuota de la app.
        if (await HayPeticionRecienteAsync(user.Id, cancellationToken)) return;

        var now = DatabaseTime.UtcNow();
        var code = GenerarCodigo();

        _db.PasswordResetCodes.Add(new Models.PasswordResetCode
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            // Hasheado, no en texto: quien lea la base no puede resetear
            // cuentas ajenas.
            CodeHash = BCrypt.Net.BCrypt.HashPassword(code),
            CreatedAt = now,
            ExpiresAt = now.Add(PasswordResetPolicy.Expiry),
        });

        await _db.SaveChangesAsync(cancellationToken);
        await _email.SendAsync(ConstruirMensaje(user.Email, code), cancellationToken);

        await LimpiarVencidosAsync(cancellationToken);
    }
}
