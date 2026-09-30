using System.Net;
using System.Text.Json;
using Conectando.Api.Tests.TestInfrastructure;

namespace Conectando.Api.Tests;

/// <summary>
/// A dónde van las respuestas del correo.
///
/// Va aparte de las pruebas del remitente porque es otra pregunta: no cómo
/// se manda el correo, sino qué pasa con el botón "Responder".
///
/// <para>
/// Importa porque el camino que deja de ir a spam es mandar desde un dominio
/// propio, y eso obliga a mandar desde una dirección que nadie mira. Con
/// ReplyTo el correo sale desde el dominio y las respuestas siguen llegando
/// a la casilla de verdad, que es lo único que importa cuando alguien
/// escribe para decir que no recibió su código.
/// </para>
/// </summary>
public sealed class BrevoEmailSenderRespuestasTests
{
    [Fact]
    public async Task Las_respuestas_van_a_la_casilla_configurada()
    {
        var handler = new FakeHttpHandler(HttpStatusCode.Created);
        var sender = BrevoEmailSenderTests.Crear(
            handler,
            "clave",
            "Conectando <no-reply@midominio.com>",
            replyTo: "Santi <santicapo64@gmail.com>");

        await sender.SendAsync(BrevoEmailSenderTests.Mensaje);

        using var cuerpo = JsonDocument.Parse(handler.Cuerpo!);
        var respuesta = cuerpo.RootElement.GetProperty("replyTo");
        Assert.Equal("santicapo64@gmail.com", respuesta.GetProperty("email").GetString());
        Assert.Equal("Santi", respuesta.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Sin_respuesta_configurada_no_manda_el_campo()
    {
        var handler = new FakeHttpHandler(HttpStatusCode.Created);
        var sender = BrevoEmailSenderTests.Crear(
            handler,
            "clave",
            "Conectando <no-reply@ejemplo.com>");

        await sender.SendAsync(BrevoEmailSenderTests.Mensaje);

        // Mandarlo vacío sería peor: Brevo toma un remitente vacío como
        // válido y las respuestas se pierden sin avisar.
        using var cuerpo = JsonDocument.Parse(handler.Cuerpo!);
        Assert.False(cuerpo.RootElement.TryGetProperty("replyTo", out _));
    }
}