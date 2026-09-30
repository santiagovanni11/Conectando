using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Conectando.Api.Interfaces;
using Conectando.Api.Settings;

namespace Conectando.Api.Services;

/// <summary>
/// Envío por la API HTTP de Brevo.
///
/// <para>
/// Va por HTTP y no por SMTP a propósito, y no es una preferencia: Render
/// bloquea el tráfico saliente hacia los puertos SMTP —25, 465 y 587— en los
/// planes gratuitos. La conexión se queda esperando hasta que expira y
/// Revisa log un <c>SmtpException: The operation has timed out</c> que no
/// dice nada de por qué. Por la API sale por el 443, que no está bloqueado.
/// </para>
///
/// Habla con Brevo y no con un proveedor genérico porque es el que está
/// configurado. Si algún día cambia, lo único que se toca es el cuerpo de
/// <see cref="SendAsync"/>; el resto de la app no se entera de nada.
/// </summary>
public sealed class BrevoEmailSender(
    HttpClient http,
    MailSettings settings,
    ILogger<BrevoEmailSender> logger) : IEmailSender
{
    private const string Endpoint = "https://api.brevo.com/v3/smtp/email";

    private static readonly JsonSerializerOptions Json = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly MailSettings _settings = settings;
    private readonly ILogger<BrevoEmailSender> _logger = logger;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_settings.ApiKey) &&
        !string.IsNullOrWhiteSpace(_settings.From);

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            // Modo prueba: sin proveedor configurado, el flujo de recuperación
            // sigue andando y el código queda a la vista en el log.
            _logger.LogWarning(
                "Envío sin configurar. Para {Destinatario}: {Asunto}\n{Cuerpo}",
                message.To,
                message.Subject,
                message.TextBody);

            return;
        }

        using var peticion = new HttpRequestMessage(HttpMethod.Post, Endpoint);

        // La clave viaja en la cabecera, no en el cuerpo ni en la URL: si
        // aparece en un log de URL, se filtra.
        peticion.Headers.Add("api-key", _settings.ApiKey);
        peticion.Headers.Add("accept", "application/json");
        peticion.Content = JsonContent.Create(
            ConstruirCuerpo(message),
            options: Json);

        using var respuesta = await http.SendAsync(peticion, cancellationToken);

        if (respuesta.IsSuccessStatusCode)
        {
            _logger.LogInformation(
                "Correo enviado a {Destinatario}: {Asunto}",
                message.To,
                message.Subject);

            return;
        }

        /*
         * El cuerpo del error dice la causa —clave inválida, remitente sin
         * verificar, cuota vencida— y sin él cualquier problema de envío es
         * un misterio. Por eso se lee y se tira, en vez de un "no se pudo".
         */
        var detalle = await respuesta.Content.ReadAsStringAsync(cancellationToken);

        throw new InvalidOperationException(
            $"Brevo respondió {(int)respuesta.StatusCode}: {Recortar(detalle)}");
    }

    private object ConstruirCuerpo(EmailMessage message)
    {
        var (nombre, correo) = SepararRemitente();

        return new
        {
            sender = new { name = nombre, email = correo },
            to = new[] { new { email = message.To } },
            subject = message.Subject,
            htmlContent = message.HtmlBody,
            textContent = message.TextBody,
        };
    }

    /// <summary>
    /// Parte el "From" de la configuración, que viene como "Nombre &lt;correo&gt;".
    /// Si no trae los ángulos, se usa tal cual: hay gente que pega solo la
    /// dirección y romper ahí sería tirar abajo el envío por un detalle de
    /// formato.
    /// </summary>
    private (string Nombre, string Correo) SepararRemitente()
    {
        var from = _settings.From;
        var abre = from.IndexOf('<');
        var cierra = from.IndexOf('>');

        if (abre < 0 || cierra < abre)
        {
            return (from, from);
        }

        return (from[..abre].Trim(), from[(abre + 1)..cierra].Trim());
    }

    /// <summary>Recorta el detalle: puede traer HTML largo y no sirve de más.</summary>
    private static string Recortar(string detalle) =>
        detalle.Length <= 400 ? detalle : detalle[..400];
}
