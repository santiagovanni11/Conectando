using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conectando.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/social/friends")]
public class FriendsController(IFriendshipService friendshipService) : ControllerBase
{
    private readonly IFriendshipService _friendshipService = friendshipService;

    [HttpGet]
    public async Task<IActionResult> GetFriends(CancellationToken cancellationToken)
    {
        return Ok(await _friendshipService.GetFriendsAsync(User.GetUserId(), cancellationToken));
    }

    [HttpDelete("{friendId:guid}")]
    public async Task<IActionResult> RemoveFriend(Guid friendId, CancellationToken cancellationToken)
    {
        await _friendshipService.RemoveFriendAsync(User.GetUserId(), friendId, cancellationToken);
        return NoContent();
    }
}