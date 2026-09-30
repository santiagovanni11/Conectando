using System.Net;
using System.Text.Json;
using Conectando.Api.Interfaces;
using Conectando.Api.Services;
using Conectando.Api.Settings;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Conectando.Api.Tests;

/// <summary>
/// Pruebas del remitente contra la API de Brevo.
///
/// El handler falso se pone ahí para poder ver qué se mandó de verdad: la
/// URL, la cabecera y el cuerpo. Con una instancia real de Brevo no se podría
/// afirmar nada sin gastarse el envío, y justo lo que importa acá —que la
/// clave no viaje en la URL, que el remitente se separe bien del nombre— se
/// verifica sin mandar nada.
/// </summary>
public sealed class BrevoEmailSenderTests
{
    private static readonly EmailMessage Mensaje = new(
        "alguien@ejemplo.com",
        "Tu código",
        "<p>Hola</p>",
        "Hola");

    [Fact]
    public async Task Manda_a_la_api_con_la_clave_en_la_cabecera()
    {
        var handler = new FakeHttpHandler(HttpStatusCode.Created);
        var sender = Crear(handler, "xkeysib-123", "Conectando <no-reply@ejemplo.com>");

        await sender.SendAsync(Mensaje);

        Assert.Equal("https://api.brevo.com/v3/smtp/email", handler.UltimaUrl);
        Assert.Equal("xkeysib-123", handler.CabeceraApiKey);

        // La clave en la URL terminaría en cualquier log de acceso. Va en
        // la cabecera, siempre.
        Assert.DoesNotContain("xkeysib-123", handler.UltimaUrl);
    }

    [Fact]
    public async Task Parte_el_remitente_en_nombre_y_correo()
    {
        var handler = new FakeHttpHandler(HttpStatusCode.Created);
        var sender = Crear(handler, "clave", "Conectando <no-reply@ejemplo.com>");

        await sender.SendAsync(Mensaje);

        using var cuerpo = JsonDocument.Parse(handler.Cuerpo!);
        Assert.Equal("no-reply@ejemplo.com", cuerpo.RootElement.GetProperty("sender").GetProperty("email").GetString());
        Assert.Equal("Conectando", cuerpo.RootElement.GetProperty("sender").GetProperty("name").GetString());
    }

    [Fact]
    public async Task Acepta_un_remitente_sin_nombre()
    {
        var handler = new FakeHttpHandler(HttpStatusCode.Created);
        var sender = Crear(handler, "clave", "no-reply@ejemplo.com");

        await sender.SendAsync(Mensaje);

        // Hay gente que pega solo la dirección. Romper ahí sería tirar abajo
        // el envío por un detalle de formato.
        using var cuerpo = JsonDocument.Parse(handler.Cuerpo!);
        Assert.Equal("no-reply@ejemplo.com", cuerpo.RootElement.GetProperty("sender").GetProperty("email").GetString());
    }

    [Fact]
    public async Task Manda_el_codigo_para_que_lo_reciba_el_usuario()
    {
        var handler = new FakeHttpHandler(HttpStatusCode.Created);
        var sender = Crear(handler, "clave", "Conectando <no-reply@ejemplo.com>");

        await sender.SendAsync(Mensaje);

        using var cuerpo = JsonDocument.Parse(handler.Cuerpo!);
        Assert.Equal("alguien@ejemplo.com", cuerpo.RootElement.GetProperty("to")[0].GetProperty("email").GetString());
        Assert.Equal("Tu código", cuerpo.RootElement.GetProperty("subject").GetString());
    }

    [Fact]
    public async Task Sin_clave_no_rompe_nada()
    {
        var handler = new FakeHttpHandler(HttpStatusCode.Created);
        var sender = Crear(handler, string.Empty, "Conectando <no-reply@ejemplo.com>");

        await sender.SendAsync(Mensaje);

        // Modo prueba: el flujo sigue y el código queda a la vista en el log.
        Assert.False(sender.IsConfigured);
        Assert.Null(handler.UltimaUrl);
    }

    [Fact]
    public async Task Anota_el_identificador_del_envio()
    {
        var handler = new FakeHttpHandler(
            HttpStatusCode.Created,
            """{"messageId":"<abc123@smtp-relay.brevo.com>"}""");
        var log = new FakeLogger<BrevoEmailSender>();
        var sender = Crear(handler, "clave", "Conectando <no-reply@ejemplo.com>", log);

        // El identificador es lo único que deja seguir el envío en los
        // registros de Brevo cuando el correo no llega. Sin esto, desde el
        // log no se distingue "aceptado" de "entregado".
        await sender.SendAsync(Mensaje);

        Assert.Contains(log.Mensajes, m => m.Contains("<abc123@smtp-relay.brevo.com>", StringComparison.Ordinal));
    }

    [Fact]
    public async Task El_error_de_brevo_dice_la_causa()
    {
        var handler = new FakeHttpHandler(
            HttpStatusCode.Unauthorized,
            """{"code":"unauthorized","message":"Key not found"}""");
        var sender = Crear(handler, "clave-vencida", "Conectando <no-reply@ejemplo.com>");

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sender.SendAsync(Mensaje));

        // Sin el detalle del error, cualquier problema de envío es un
        // misterio y hay que adivinar probando claves.
        Assert.Contains("401", error.Message);
        Assert.Contains("Key not found", error.Message);
    }

    private static BrevoEmailSender Crear(
        HttpMessageHandler handler,
        string apiKey,
        string from,
        ILogger<BrevoEmailSender>? log = null) =>
        new(
            new HttpClient(handler),
            new MailSettings { ApiKey = apiKey, From = from },
            log ?? NullLogger<BrevoEmailSender>.Instance);
}
