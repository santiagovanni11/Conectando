namespace Conectando.Api.Settings;

/// <summary>
/// Datos públicos del sitio. Se usan para armar las vistas previas de Open
/// Graph: las URLs tienen que ser absolutas o el crawler no puede
/// descargar la imagen.
/// </summary>
public class SiteSettings
{
    public const string SectionName = "Site";

    /// <summary>Origen público, sin barra final. Ej: https://conectando.app</summary>
    public string Url { get; set; } = "http://localhost:5025";

    public string Name { get; set; } = "Conectando";

    public string Description { get; set; } =
        "La red social donde las personas se encuentran.";
}