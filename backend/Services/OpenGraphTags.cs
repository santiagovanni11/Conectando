using System.Net;
using Conectando.Api.DTOs.Previews;
using Conectando.Api.Settings;

namespace Conectando.Api.Services;

/// <summary>
/// Arma el bloque de metadatos que se inyecta en el HTML.
///
/// Va aparte del middleware a propósito: es lógica pura y se puede probar
/// sin levantar una petición HTTP. El dominio y el nombre salen de la
/// configuración, no del código.
/// </summary>
public static class OpenGraphTags
{
    /// <summary>Marcador que el middleware busca dentro del index.html.</summary>
    public const string Placeholder = "<!--og-meta-->";

    /// <summary>
    /// Metadatos por defecto, para el home y para cuando no hay nada
    /// mejor que mostrar.
    /// </summary>
    public static PagePreviewDto Defaults(SiteSettings site) => new()
    {
        Title = site.Name,
        Description = site.Description,
        Type = "website",
        CanonicalUrl = "/",
    };

    public static string Build(PagePreviewDto preview, SiteSettings site)
    {
        var title = Escape(preview.Title);
        var description = Escape(preview.Description);
        var url = Escape(AbsoluteUrl(preview.CanonicalUrl, site.Url));

        var tags = new List<string>
        {
            Meta("og:type", preview.Type),
            Meta("og:site_name", Escape(site.Name)),
            Meta("og:title", title),
            Meta("og:description", description),
            Meta("og:url", url),
            Meta("name:description", description),
            Meta("name:twitter:card", preview.ImageUrl is null ? "summary" : "summary_large_image"),
            Meta("name:twitter:title", title),
            Meta("name:twitter:description", description),
        };

        if (!string.IsNullOrWhiteSpace(preview.ImageUrl))
        {
            tags.Add(Meta("og:image", Escape(AbsoluteUrl(preview.ImageUrl, site.Url))));
        }

        tags.Add($"<title>{title}</title>");

        return string.Join("\n    ", tags);
    }

    /// <summary>
    /// Las vistas previas necesitan una URL absoluta: si se comparte en
    /// WhatsApp, un "/posts/123" solo no alcanza para descargar la imagen.
    /// </summary>
    public static string AbsoluteUrl(string? url, string siteUrl)
    {
        if (string.IsNullOrWhiteSpace(url)) return siteUrl;

        if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return url;

        return $"{siteUrl.TrimEnd('/')}/{url.TrimStart('/')}";
    }

    /// <summary>
    /// Escapa el valor para poder ponerlo dentro de un atributo HTML.
    /// Sin esto, un texto con comillas rompería el HTML que se sirve.
    /// </summary>
    private static string Escape(string value) =>
        WebUtility.HtmlEncode(value ?? string.Empty);

    private static string Meta(string propertyOrName, string content) =>
        propertyOrName.Contains(':')
            ? $"<meta property=\"{propertyOrName}\" content=\"{content}\" />"
            : $"<meta name=\"{propertyOrName}\" content=\"{content}\" />";
}