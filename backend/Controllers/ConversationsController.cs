using Conectando.Api.DTOs.Counters;
using Conectando.Api.DTOs.Messages;
using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Conectando.Api.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Conectando.Api.Controllers;

[ApiController]
[Route("api/conversations")]
[Authorize]
public class ConversationsController(
    IConversationService conversationService,
    INavCountService navCountService,
    INavCountsBroadcaster navCounts) : ControllerBase
{
    private readonly IConversationService _conversationService = conversationService;
    private readonly INavCountService _navCountService = navCountService;
    private readonly INavCountsBroadcaster _navCounts = navCounts;

    /// <summary>Contadores de aviso para los íconos de la navegación.</summary>
    [HttpGet("nav-counts")]
    public async Task<ActionResult<NavCountsDto>> GetNavCounts(CancellationToken cancellationToken)
    {
        var result = await _navCountService.GetCountsAsync(User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<ConversationDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _conversationService.GetConversationsAsync(User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ConversationDto>> StartDirect(
        StartConversationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _conversationService.StartDirectAsync(
            User.GetUserId(), request.RecipientId, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}/messages")]
    public async Task<ActionResult<MessagePageDto>> GetMessages(
        Guid id,
        [FromQuery] string? cursor = null,
        [FromQuery] int limit = 30,
        CancellationToken cancellationToken = default)
    {
        var result = await _conversationService.GetMessagesAsync(
            User.GetUserId(), id, cursor, limit, cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/messages")]
    [EnableRateLimiting(RateLimitPolicies.Messages)]
    public async Task<ActionResult<MessageDto>> SendMessage(
        Guid id,
        SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _conversationService.SendMessageAsync(
            userId, id, request.Content, request.ReplyToMessageId, cancellationToken);

        // Mismo motivo que en "marcar leido": el front manda por REST cuando
        // Mismo motivo que en "marcar leido": el front manda por REST cuando
        var peerIds = await _conversationService.GetPeerIdsAsync(id, userId);
        await _navCounts.NotifyManyAsync(peerIds.Append(userId), cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await _conversationService.MarkAsReadAsync(userId, id, cancellationToken);

        // El hub lo hace cuando el marcado es por ahí, pero el front lo manda
        // por REST cuando el hub no está. Sin esto, abrir el chat esperando
        // un número que nunca baja.
        await _navCounts.NotifyAsync(userId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Borra el chat solo para quien lo pide. La otra persona lo conserva,
    /// igual que en WhatsApp.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteConversation(Guid id, CancellationToken cancellationToken)
    {
        await _conversationService.DeleteConversationAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/messages/{messageId:guid}")]
    [EnableRateLimiting(RateLimitPolicies.Messages)]
    public async Task<ActionResult<MessageDto>> EditMessage(
        Guid id,
        Guid messageId,
        EditMessageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _conversationService.EditMessageAsync(
            User.GetUserId(), id, messageId, request.Content, cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/mute")]
    [EnableRateLimiting(RateLimitPolicies.Messages)]
    public async Task<ActionResult<MuteConversationResponse>> Mute(
        Guid id,
        MuteConversationRequest request,
        CancellationToken cancellationToken)
    {
        var muted = await _conversationService.SetMutedAsync(
            User.GetUserId(), id, request.Muted, cancellationToken);

        return Ok(new MuteConversationResponse { IsMuted = muted });
    }

    [HttpDelete("{id:guid}/messages/{messageId:guid}")]
    [EnableRateLimiting(RateLimitPolicies.Messages)]
    public async Task<ActionResult<MessageDto>> DeleteMessage(
        Guid id,
        Guid messageId,
        CancellationToken cancellationToken)
    {
        var result = await _conversationService.DeleteMessageAsync(
            User.GetUserId(), id, messageId, cancellationToken);

        return Ok(result);
    }
}
