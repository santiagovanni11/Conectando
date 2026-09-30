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
public sealed partial class BrevoEmailSender(
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
            // Brevo devuelve un messageId. Es el único dato que permite
            // seguir un envío concreto en sus registros cuando el correo no
            // llega, así que queda anotado. "Enviado" solo quiere decir que
            // Brevo lo aceptó: de acá en más decides Gmail, Yahoo y la
            // reputación del remitente, y nada de eso se ve desde acá.
            _logger.LogInformation(
                "Correo aceptado por Brevo para {Destinatario} ({MessageId}): {Asunto}",
                message.To,
                await LeerMessageId(respuesta, cancellationToken),
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

            // Sin replyTo, el "Responder" le llega a la dirección técnica del
            // dominio. Con ReplyTo sigue yendo a la casilla de verdad. Es lo
            // que hace posible mandar desde un dominio —que es lo único que
            // pasa los filtros de Gmail— sin perder los correos que
            // alguien conteste.
            replyTo = ConstruirReplyTo(),

            to = new[] { new { email = message.To } },
            subject = message.Subject,
            htmlContent = message.HtmlBody,
            textContent = message.TextBody,
        };
    }
}
