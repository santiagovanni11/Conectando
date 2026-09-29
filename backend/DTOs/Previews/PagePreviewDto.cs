namespace Conectando.Api.DTOs.Previews;

/// <summary>
/// Datos de la vista previa de una página (Open Graph). Los_extractores
/// de WhatsApp, Facebook y Slack no ejecutan JavaScript, así que el HTML
/// tiene que llegar con estos datos ya puestos.
/// </summary>
public class PagePreviewDto
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string CanonicalUrl { get; init; } = string.Empty;
    public string Type { get; init; } = "website";
}