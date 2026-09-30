using System.Net;
using Microsoft.Extensions.Logging;

namespace Conectando.Api.Tests.TestInfrastructure;

/// <summary>
/// Handler falso que anota lo que le llega y contesta lo que se le pida, sin
/// tocar la red.
///
/// Existe para poder afirmar sobre lo que realmente se manda —la URL, la
/// cabecera, el cuerpo— sin gastar un envío de verdad. Con el proveedor
/// real no se podría comprobar nada, y justo lo que importa en un correo
/// —que la clave no viaje en la URL, que el remitente esté bien partido— se
/// verifica sin mandar nada.
/// </summary>
public sealed class FakeHttpHandler(HttpStatusCode status, string? cuerpo = null)
    : HttpMessageHandler
{
    public string? UltimaUrl { get; private set; }

    public string? CabeceraApiKey { get; private set; }

    public string? Cuerpo { get; private set; }

    /// <summary>Lo que contesta, que es donde puede venir el identificador.</summary>
    public string? CuerpoRespuesta { get; } = cuerpo;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        UltimaUrl = request.RequestUri?.ToString();
        request.Headers.TryGetValues("api-key", out var valores);
        CabeceraApiKey = valores?.SingleOrDefault();
        Cuerpo = await request.Content!.ReadAsStringAsync(cancellationToken);

        return new HttpResponseMessage(status)
        {
            Content = new StringContent(CuerpoRespuesta ?? string.Empty),
        };
    }
}

/// <summary>
/// Logger que guarda lo que se escribe.
///
/// Hace falta cuando lo que se quiere comprobar solo existe si pasó por el
/// log: no hay forma de verificar desde afuera que un identificador quedó
/// anotado, porque no se devuelve, se escribe.
/// </summary>
public sealed class FakeLogger<T> : ILogger<T>
{
    public List<string> Mensajes { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) =>
        Mensajes.Add(formatter(state, exception));
}