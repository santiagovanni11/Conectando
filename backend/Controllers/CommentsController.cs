using Conectando.Api.DTOs.Comments;
using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Conectando.Api.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Conectando.Api.Controllers;

[ApiController]
[Route("api/posts/{postId:guid}/comments")]
public class CommentsController(ICommentService commentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CommentPageDto>> GetComments(
        Guid postId,
        [FromQuery] string? cursor,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await commentService.GetCommentsAsync(postId, userId, cursor, limit, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    public async Task<ActionResult<CommentDto>> CreateComment(
        Guid postId,
        [FromBody] CreateCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await commentService.CreateCommentAsync(postId, userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetComments), new { postId }, result);
    }
}

[ApiController]
[Route("api/comments/{commentId:guid}")]
public class CommentRepliesController(ICommentService commentService) : ControllerBase
{
    [HttpGet("replies")]
    public async Task<ActionResult<IReadOnlyList<CommentDto>>> GetReplies(
        Guid commentId,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await commentService.GetRepliesAsync(commentId, userId, limit, cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<CommentDto>> UpdateComment(
        Guid commentId,
        [FromBody] UpdateCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await commentService.UpdateCommentAsync(commentId, userId, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteComment(Guid commentId, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        await commentService.DeleteCommentAsync(commentId, userId, cancellationToken);
        return NoContent();
    }
}