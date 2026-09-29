using Conectando.Api.Middleware;
using Microsoft.AspNetCore.Http;

namespace Conectando.Api.Tests;

/// <summary>
/// Qué rutas intercepta el middleware de Open Graph.
/// </summary>
/// <remarks>
/// El bug que estos tests cubren era invisible en desarrollo y dejaba la app
/// en blanco en producción: el navegador pedía el bundle de JavaScript y
/// recibía el index.html, porque el middleware se llevaba todo GET que no
/// fuera /api. En desarrollo nunca se ve, porque ahí Vite sirve los assets y
/// el backend no participa.
/// </remarks>
public class SpaMetadataMiddlewareTests
{
    private static bool ShouldHandle(string path, string method = "GET")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        return SpaMetadataMiddleware.ShouldHandle(context.Request);
    }

    [Theory]
    [InlineData("/assets/index-abc123.js")]
    [InlineData("/assets/index-abc123.css")]
    [InlineData("/favicon.svg")]
    [InlineData("/ImagenConectando.webp")]
    public void Archivos_NoLosIntercepta(string path)
    {
        // Un archivo con extensión no es una página. Si el middleware lo
        // intercepta contesta HTML donde el navegador esperaba JavaScript, y
        // lo único que se ve es una pantalla en blanco.
        Assert.False(ShouldHandle(path));
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/login")]
    [InlineData("/register")]
    [InlineData("/profile")]
    [InlineData("/messages")]
    [InlineData("/post/6f1b2c3d-0000-0000-0000-000000000000")]
    public void RutasDePagina_LasIntercepta(string path)
    {
        // Estas sí las tiene que servir, con los metadatos ya puestos: es lo
        // único que hace que el preview de WhatsApp muestre la foto.
        Assert.True(ShouldHandle(path));
    }

    [Theory]
    [InlineData("/api/posts")]
    [InlineData("/hubs/messages")]
    public void ApiYHub_NoLosIntercepta(string path)
    {
        Assert.False(ShouldHandle(path));
    }

    [Fact]
    public void Otras_que_no_sean_GET_NoLasIntercepta()
    {
        // Un POST a una ruta de página es la API, no la SPA.
        Assert.False(ShouldHandle("/login", "POST"));
    }
}
