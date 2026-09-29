using Conectando.Api.DTOs.Posts;
using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conectando.Api.Controllers;

[ApiController]
[Route("api/users/{userId:guid}/posts")]
[Authorize]
public class UserPostsController(IPostReadService readService) : ControllerBase
{
    private readonly IPostReadService _readService = readService;

    [HttpGet]
    public async Task<ActionResult<PostListDto>> ListPosts(
        Guid userId,
        [FromQuery] string? cursor,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var viewerId = User.GetUserId();
        var list = await _readService.ListByUserAsync(userId, viewerId, new PostListQuery { Cursor = cursor, Limit = limit }, cancellationToken);
        return Ok(list);
    }
}