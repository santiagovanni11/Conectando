using Conectando.Api.Data;
using Conectando.Api.DTOs.Messages;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public partial class ConversationService
{
    public async Task<MessagePageDto> GetMessagesAsync(
        Guid userId,
        Guid conversationId,
        string? cursor,
        int limit,
        CancellationToken cancellationToken = default)
    {
        await EnsureIsMemberAsync(userId, conversationId, cancellationToken);
        var take = Math.Clamp(limit, 1, MaxLimit);
        var parsed = MessageCursorParser.Parse(cursor);

        var query = _dbContext.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId);

        if (parsed is not null)
        {
            query = query.Where(m =>
                m.CreatedAt < parsed.CreatedAt ||
                (m.CreatedAt == parsed.CreatedAt && m.Id.CompareTo(parsed.Id) < 0));
        }

        var rows = await query
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .Take(take + 1)
            .Select(m => new MessageRow(
                m.Id,
                m.ConversationId,
                m.Content,
                m.CreatedAt,
                m.SenderId,
                m.Sender.UserName,
                m.Sender.DisplayName,
                m.Sender.ProfileImageUrl,
                m.EditedAt,
                m.IsDeleted))
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > take;
        var page = hasMore ? rows[..take] : rows;

        // El "visto" depende de hasta dónde leyó el otro, no de un campo
        // por mensaje: se trae una sola vez y se compara con cada fecha.
        var peerLastRead = await _dbContext.ConversationMembers
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId && m.UserId != userId)
            .Select(m => m.LastReadAt)
            .FirstOrDefaultAsync(cancellationToken);

        var seenCutoff = peerLastRead ?? DateTime.MinValue;

        return new MessagePageDto
        {
            HasMore = hasMore,
            Items = [.. page.Select(row => MapMessage(row, seenCutoff))],
            NextCursor = hasMore && page.Count > 0
                ? MessageCursorParser.Encode(page[^1].CreatedAt, page[^1].Id)
                : null,
        };
    }
}
