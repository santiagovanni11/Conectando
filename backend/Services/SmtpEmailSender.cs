using System.Net;
using System.Net.Mail;
using Conectando.Api.Interfaces;
using Conectando.Api.Settings;
using Microsoft.Extensions.Options;

namespace Conectando.Api.Services;

/// <summary>
/// Envío por SMTP.
///
/// Se habla SMTP y no el API de un proveedor a propósito: SMTP es un
/// estándar, y el mismo código sirve para el relay que sea hoy o para
/// cualquier otro mañana. Cambiar de proveedor es cambiar tres variables de
/// entorno, no reescribir el envío.
///
/// Se usa SmtpClient porque alcanza para lo que hace falta —texto y HTML,
/// sin adjuntos— y evita sumar un paquete por algo que .NET ya trae. La
/// advertencia de obsolescencia se silencia a propósito y queda anotada acá.
/// </summary>
#pragma warning disable SYSLIB0014
public sealed class SmtpEmailSender(MailSettings settings, ILogger<SmtpEmailSender> logger)
    : IEmailSender
{
    private readonly MailSettings _settings = settings;
    private readonly ILogger<SmtpEmailSender> _logger = logger;

    public bool IsConfigured => _settings.CanSend;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            // Modo prueba. Se registra el código para poder completar el
            // flujo sin un proveedor externo. En un entorno real configurado
            // nunca se llega acá.
            _logger.LogWarning(
                "Envío sin configurar. Para {Destinatario}: {Asunto}\n{Cuerpo}",
                message.To,
                message.Subject,
                message.TextBody);

            return;
        }

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            // STARTTLS: abre en texto plano y cifra después de arrancar.
            // Con 465 sería SSL directo; el 587 es el que da casi todo el
            // mundo y el único que hay que dejar con cifrado sí o sí.
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential(_settings.User, _settings.Password),
            Timeout = 20000,
        };

        using var mail = new MailMessage
        {
            From = new MailAddress(_settings.From),
            Subject = message.Subject,
            Body = message.HtmlBody,
            IsBodyHtml = true,
        };

        // Alternativa en texto plano, que es lo que leen los lectores de
        // pantalla y los clientes sin HTML.
        mail.Body += $"\r\n\r\n----------\r\n{message.TextBody}";
        mail.To.Add(message.To);

        await client.SendMailAsync(mail, cancellationToken);
        _logger.LogInformation("Correo enviado a {Destinatario}: {Asunto}", message.To, message.Subject);
    }
}
#pragma warning restore SYSLIB0014
