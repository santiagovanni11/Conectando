using Conectando.Api.DTOs.Notifications;
using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conectando.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(INotificationService notificationService) : ControllerBase
{
    private readonly INotificationService _notificationService = notificationService;

    [HttpGet]
    public async Task<ActionResult<NotificationPageDto>> GetAll(
        [FromQuery] string? cursor = null,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.GetAllAsync(User.GetUserId(), cursor, limit, cancellationToken);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadCountDto>> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.GetUnreadCountAsync(User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken = default)
    {
        await _notificationService.MarkAsReadAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        await _notificationService.MarkAllAsReadAsync(User.GetUserId(), cancellationToken);
        return NoContent();
    }
}