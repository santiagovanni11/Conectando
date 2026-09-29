using Conectando.Api.DTOs.Posts;
using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conectando.Api.Controllers;

[ApiController]
[Route("api/posts/{postId:guid}/likes")]
[Authorize]
public class PostLikesListController(ILikeService likeService) : ControllerBase
{
    private readonly ILikeService _likeService = likeService;

    [HttpGet]
    public async Task<ActionResult<LikePageDto>> GetLikes(
        Guid postId,
        [FromQuery] string? cursor = null,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _likeService.GetLikesAsync(postId, User.GetUserId(), cursor, limit, cancellationToken);
        return Ok(result);
    }
}