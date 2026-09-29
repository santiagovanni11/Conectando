using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conectando.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/social")]
public class FollowsController(IFollowService followService) : ControllerBase
{
    private readonly IFollowService _followService = followService;

    [HttpGet("followers")]
    public async Task<IActionResult> GetFollowers(CancellationToken cancellationToken)
    {
        return Ok(await _followService.GetFollowersAsync(User.GetUserId(), cancellationToken));
    }

    [HttpGet("following")]
    public async Task<IActionResult> GetFollowing(CancellationToken cancellationToken)
    {
        return Ok(await _followService.GetFollowingAsync(User.GetUserId(), cancellationToken));
    }

    [HttpPost("follow/{userId:guid}")]
    public async Task<IActionResult> Follow(Guid userId, CancellationToken cancellationToken)
    {
        await _followService.FollowAsync(User.GetUserId(), userId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("follow/{userId:guid}")]
    public async Task<IActionResult> Unfollow(Guid userId, CancellationToken cancellationToken)
    {
        await _followService.UnfollowAsync(User.GetUserId(), userId, cancellationToken);
        return NoContent();
    }
}