using Conectando.Api.DTOs.Posts;
using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Conectando.Api.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Conectando.Api.Controllers;

[ApiController]
[Route("api/posts")]
[Authorize]
public class PostsController(IPostWriteService writeService, IPostReadService readService, IPostPatchService patchService) : ControllerBase
{
    private readonly IPostWriteService _writeService = writeService;
    private readonly IPostReadService _readService = readService;
    private readonly IPostPatchService _patchService = patchService;

    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    public async Task<ActionResult<PostDto>> CreatePost(CreatePostRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var post = await _writeService.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostDto>> GetPost(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var post = await _readService.GetByIdAsync(id, userId, cancellationToken);
        return Ok(post);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PostDto>> UpdatePost(Guid id, UpdatePostRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var post = await _writeService.UpdateAsync(id, userId, request, cancellationToken);
        return Ok(post);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<PostDto>> PatchPost(Guid id, PatchPostRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var post = await _patchService.PatchAsync(id, userId, request, cancellationToken);
        return Ok(post);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePost(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await _writeService.DeleteAsync(id, userId, cancellationToken);
        return NoContent();
    }
}