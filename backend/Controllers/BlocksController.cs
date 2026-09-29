using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conectando.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/social")]
public class BlocksController(IBlockService blockService) : ControllerBase
{
    private readonly IBlockService _blockService = blockService;

    [HttpGet("blocks")]
    public async Task<IActionResult> GetBlocks(CancellationToken cancellationToken)
    {
        return Ok(await _blockService.GetBlocksAsync(User.GetUserId(), cancellationToken));
    }

    [HttpPost("block/{userId:guid}")]
    public async Task<IActionResult> Block(Guid userId, CancellationToken cancellationToken)
    {
        await _blockService.BlockAsync(User.GetUserId(), userId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("block/{userId:guid}")]
    public async Task<IActionResult> Unblock(Guid userId, CancellationToken cancellationToken)
    {
        await _blockService.UnblockAsync(User.GetUserId(), userId, cancellationToken);
        return NoContent();
    }
}