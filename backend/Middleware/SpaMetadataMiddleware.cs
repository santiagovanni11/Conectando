using System.IO;
using System.Text;
using Conectando.Api.DTOs.Previews;
using Conectando.Api.Interfaces;
using Conectando.Api.Services;
using Conectando.Api.Settings;
using Microsoft.Extensions.Options;

namespace Conectando.Api.Middleware;

/// <summary>
/// Sirve la SPA con los metadatos de Open Graph ya puestos.
///
/// No es SSR: la app sigue siendo la misma de siempre. Lo único que cambia
/// es que el HTML lleva los &lt;meta&gt; correctos, porque los lectores de
/// links (WhatsApp, Slack, Twitter) no ejecutan JavaScript y nunca verían
/// los que pone el frontend.
///
/// Solo atiende GET sobre rutas de contenido. Cualquier otra petición sigue
/// su curso normal hacia la API o los archivos estáticos.
/// </summary>
public sealed class SpaMetadataMiddleware(RequestDelegate next, ILogger<SpaMetadataMiddleware> logger)
{
    // El shell se lee una vez por proceso: en cada petición volver al disco
    // sería un desperdicio, y el HTML no cambia mientras no se redespliegue.
    private static string? _shell;

    private readonly RequestDelegate _next = next;
    private readonly ILogger<SpaMetadataMiddleware> _logger = logger;

    public async Task InvokeAsync(
        HttpContext context,
        IPagePreviewService previewService,
        IOptions<SiteSettings> siteOptions)
    {
        if (!ShouldHandle(context.Request))
        {
            await _next(context);
            return;
        }

        var shell = await ReadShellAsync(context);
        if (shell is null)
        {
            // En desarrollo el index.html lo sirve Vite, no el backend.
            await _next(context);
            return;
        }

        var site = siteOptions.Value;
        var preview = await ResolveAsync(context, previewService, site);
        var html = shell.Replace(OpenGraphTags.Placeholder, OpenGraphTags.Build(preview, site), StringComparison.Ordinal);

        await WriteAsync(context, html);
    }

    // Visible para los tests: decide qué rutas se sirven con metadatos y cuáles
    // dejan pasar, y esa decisión no necesita disco ni base para probarse.
    internal static bool ShouldHandle(HttpRequest request) =>
        HttpMethods.IsGet(request.Method)
        && !request.Path.StartsWithSegments("/api")
        && !request.Path.StartsWithSegments("/hubs")
        // Cualquier cosa con extensión es un archivo, no una página. Sin esto
        // el navegador pide /assets/app.js y recibe el index.html, que es
        // JavaScript inválido: la app queda en blanco.
        && !Path.HasExtension(request.Path);

    private async Task WriteAsync(HttpContext context, string html)
    {
        // El index nunca se cachea: si se cacheara, un crawler leería los
        // metadatos viejos. Los assets con hash sí se cachean aparte.
        context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.ContentLength = Encoding.UTF8.GetByteCount(html);

        await context.Response.WriteAsync(html, Encoding.UTF8);
    }

    private async Task<PagePreviewDto> ResolveAsync(
        HttpContext context,
        IPagePreviewService previewService,
        SiteSettings site)
    {
        var segments = context.Request.Path.Value?.Trim('/').Split('/') ?? [];

        if (segments.Length == 2 && Guid.TryParse(segments[1], out var id))
        {
            try
            {
                var viewer = GetViewerId(context);
                var preview = segments[0] switch
                {
                    "posts" => await previewService.GetPostAsync(id, viewer),
                    "users" => await previewService.GetProfileAsync(id),
                    _ => null,
                };

                if (preview is not null) return preview;
            }
            catch (Exception exception)
            {
                // Un preview que falla no puede tumbar la página: se sirve
                // la versión genérica y se registra para diagnóstico.
                _logger.LogWarning(exception, "No se pudo generar la vista previa de {Path}", context.Request.Path);
            }
        }

        return OpenGraphTags.Defaults(site);
    }

    private static Guid GetViewerId(HttpContext context)
    {
        var value = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }

    /// <summary>
    /// Lee el index.html del wwwroot. Se cachea en memoria: se pide una vez
    /// por proceso, no en cada petición.
    /// </summary>
    private static async Task<string?> ReadShellAsync(HttpContext context)
    {
        if (_shell is not null) return _shell;

        var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var path = Path.Combine(env.WebRootPath ?? string.Empty, "index.html");

        if (!File.Exists(path)) return null;

        _shell = await File.ReadAllTextAsync(path);
        return _shell;
    }
}