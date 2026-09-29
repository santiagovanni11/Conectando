using Conectando.Api.DTOs.Posts;
using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conectando.Api.Controllers;

[ApiController]
[Route("api/feed")]
[Authorize]
public class FeedController(IFeedService feedService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PostListDto>> GetFeed(
        [FromQuery] string? cursor,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var viewerId = User.GetUserId();
        var page = await feedService.GetPageAsync(viewerId, cursor, limit, cancellationToken);
        return Ok(page);
    }
}
