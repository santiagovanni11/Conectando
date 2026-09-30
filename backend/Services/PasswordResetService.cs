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

    private readonly ILogger<PasswordResetService> _logger;

    public PasswordResetService(
        ConectandoDbContext db,
        IEmailSender email,
        MailSettings settings,
        ILogger<PasswordResetService> logger)
    {
        _db = db;
        _email = email;
        _marca = settings.BrandName;
        _logger = logger;
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
        await EnviarSinRomper(user.Email, code, cancellationToken);

        await LimpiarVencidosAsync(cancellationToken);
    }

    /// <summary>
    /// Manda el código sin que un fallo del proveedor rompa la respuesta.
    ///
    /// El código ya está guardado cuando se llega acá, así que la operación
    /// se hizo. Que el proveedor falle —una clave mal puesta, una cuota
    /// vencida, un servicio caído— no la deshace, y el usuario no puede
    /// hacer nada al respecto: puede volver a pedirlo en un minuto y pasar
    /// por lo mismo.
    ///
    /// Y sobre todo, dejarlo escapar rompía la regla más importante del
    /// flujo: con el envío caído, un correo registrado daba 500 y uno
    /// inexistente daba 202. Con solo mirar el status se armaba la lista de
    /// quién tiene cuenta, que es justo lo que el resto del método evita.
    ///
    /// Se registra con el detalle de la excepción para que el problema se
    /// pueda diagnosticar del lado del servidor.
    /// </summary>
    private async Task EnviarSinRomper(
        string correo,
        string codigo,
        CancellationToken cancellationToken)
    {
        try
        {
            await _email.SendAsync(ConstruirMensaje(correo, codigo), cancellationToken);
        }
        catch (Exception cause)
        {
            _logger.LogError(
                cause,
                "No se pudo mandar el código de recuperación a {Correo}. Configurá MailSettings.",
                correo);
        }
    }
}
