using Conectando.Api.DTOs.Previews;

namespace Conectando.Api.Interfaces;

/// <summary>
/// Datos para la vista previa de una página. Existen porque los lectores de
/// links (WhatsApp, Slack, Twitter) no ejecutan JavaScript: si los datos
/// los pone el frontend, no llegan a verse nunca.
/// </summary>
public interface IPagePreviewService
{
    Task<PagePreviewDto?> GetPostAsync(
        Guid postId, Guid viewerId, CancellationToken cancellationToken = default);

    Task<PagePreviewDto?> GetProfileAsync(
        Guid userId, CancellationToken cancellationToken = default);
}