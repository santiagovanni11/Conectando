using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conectando.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/social/requests")]
public class FriendRequestsController(IFriendRequestService friendRequestService) : ControllerBase
{
    private readonly IFriendRequestService _friendRequestService = friendRequestService;

    [HttpGet("received")]
    public async Task<IActionResult> GetReceived(CancellationToken cancellationToken)
    {
        return Ok(await _friendRequestService.GetReceivedAsync(User.GetUserId(), cancellationToken));
    }

    [HttpGet("sent")]
    public async Task<IActionResult> GetSent(CancellationToken cancellationToken)
    {
        return Ok(await _friendRequestService.GetSentAsync(User.GetUserId(), cancellationToken));
    }

    [HttpPost("{userId:guid}")]
    public async Task<IActionResult> Send(Guid userId, CancellationToken cancellationToken)
    {
        await _friendRequestService.SendAsync(User.GetUserId(), userId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{userId:guid}/accept")]
    public async Task<IActionResult> Accept(Guid userId, CancellationToken cancellationToken)
    {
        await _friendRequestService.AcceptAsync(User.GetUserId(), userId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{userId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid userId, CancellationToken cancellationToken)
    {
        await _friendRequestService.RejectAsync(User.GetUserId(), userId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Cancel(Guid userId, CancellationToken cancellationToken)
    {
        await _friendRequestService.CancelAsync(User.GetUserId(), userId, cancellationToken);
        return NoContent();
    }
}