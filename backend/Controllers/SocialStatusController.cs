using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conectando.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/social")]
public class SocialStatusController(ISocialStatusService socialStatusService) : ControllerBase
{
    private readonly ISocialStatusService _socialStatusService = socialStatusService;

    [HttpGet("status/{userId:guid}")]
    public async Task<IActionResult> GetStatus(Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await _socialStatusService.GetStatusAsync(User.GetUserId(), userId, cancellationToken));
    }
}