using Conectando.Api.DTOs.Posts;
using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Conectando.Api.RateLimiting;
using Conectando.Api.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Conectando.Api.Controllers;

[ApiController]
[Route("api/posts/media")]
[Authorize]
public class PostMediaController(IPostMediaService mediaService) : ControllerBase
{
    private readonly IPostMediaService _mediaService = mediaService;

    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.Uploads)]
    [RequestSizeLimit(PostConstants.MaxUploadRequestBodyBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = PostConstants.MaxUploadRequestBodyBytes)]
    public async Task<ActionResult<List<PostMediaDto>>> UploadMedia(
        [FromForm] IFormFile[] files,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var media = await _mediaService.UploadAsync(userId, files, cancellationToken);
        return Ok(media);
    }

    [HttpDelete("{mediaId:guid}")]
    [EnableRateLimiting(RateLimitPolicies.Uploads)]
    public async Task<IActionResult> DeleteMedia(Guid mediaId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await _mediaService.DeleteAsync(mediaId, userId, cancellationToken);
        return NoContent();
    }
}