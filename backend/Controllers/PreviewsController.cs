using Conectando.Api.DTOs.Previews;
using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Conectando.Api.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conectando.Api.Controllers;

/// <summary>
/// Sirve la página con los metadatos de Open Graph ya puestos.
///
/// No hace falta SSR completo: los lectores de links no ejecutan
/// JavaScript, así que alcanza con devolver el mismo index.html pero con
/// los &lt;meta&gt; correctos. El resto de la navegación sigue siendo la
/// SPA normal.
/// </summary>
[ApiController]
[Route("api/previews")]
[AllowAnonymous]
public class PreviewsController(IPagePreviewService previewService) : ControllerBase
{
    private readonly IPagePreviewService _previewService = previewService;

    [HttpGet("post/{postId:guid}")]
    public async Task<ActionResult<PagePreviewDto>> GetPost(Guid postId, CancellationToken cancellationToken)
    {
        var viewer = TryGetUserId();
        var result = await _previewService.GetPostAsync(postId, viewer, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("profile/{userId:guid}")]
    public async Task<ActionResult<PagePreviewDto>> GetProfile(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _previewService.GetProfileAsync(userId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    private Guid TryGetUserId()
    {
        var value = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }
}