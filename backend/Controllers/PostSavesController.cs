using Conectando.Api.DTOs.Posts;
using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Conectando.Api.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Conectando.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/posts/saved")]
public class PostSavesController(IPostSaveService saveService) : ControllerBase
{
    private readonly IPostSaveService _saveService = saveService;

    /// <summary>Guarda la publicación si no estaba, y la quita si estaba.</summary>
    [HttpPost("{postId:guid}")]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    public async Task<ActionResult<PostSaveStatusDto>> Toggle(
        Guid postId, CancellationToken cancellationToken)
    {
        return Ok(await _saveService.ToggleAsync(User.GetUserId(), postId, cancellationToken));
    }

    /// <summary>Las publicaciones que guardó el usuario actual.</summary>
    [HttpGet]
    public async Task<ActionResult<PostListDto>> ListSaved(
        [FromQuery] PostListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _saveService.ListSavedAsync(User.GetUserId(), query, cancellationToken));
    }
}