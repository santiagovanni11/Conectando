using Conectando.Api.DTOs.Posts;
using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conectando.Api.Controllers;

[ApiController]
[Route("api/posts/{postId:guid}/like")]
[Authorize]
public class PostLikesController(ILikeService likeService) : ControllerBase
{
    private readonly ILikeService _likeService = likeService;

    [HttpPost]
    public async Task<ActionResult<PostLikeStatusDto>> Like(Guid postId, CancellationToken cancellationToken)
    {
        var status = await _likeService.LikeAsync(postId, User.GetUserId(), cancellationToken);
        return Ok(status);
    }

    [HttpDelete]
    public async Task<ActionResult<PostLikeStatusDto>> Unlike(Guid postId, CancellationToken cancellationToken)
    {
        var status = await _likeService.UnlikeAsync(postId, User.GetUserId(), cancellationToken);
        return Ok(status);
    }

    [HttpGet]
    public async Task<ActionResult<PostLikeStatusDto>> GetStatus(Guid postId, CancellationToken cancellationToken)
    {
        var status = await _likeService.GetStatusAsync(postId, User.GetUserId(), cancellationToken);
        return Ok(status);
    }
}