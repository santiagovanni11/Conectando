using Conectando.Api.DTOs.Messages;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Traducción de una fila de la base al mensaje que ve el front.
/// </summary>
public partial class ConversationService
{
    /// <summary>
    /// Caracteres que se envían de la cita. El mensaje puede tener 2000, pero
    /// la cita se muestra en una línea: mandar el texto entero multiplicaría
    /// el peso de cada respuesta sin aportar nada visible.
    /// </summary>
    private const int ReplyPreviewMaxLength = 120;

    private static MessageReplyDto? MapReply(MessageRow row)
    {
        // Sin id no hay cita. El nombre y el texto pueden venir vacíos sin
        // que eso invalide la referencia.
        if (row.ReplyToMessageId is null) return null;

        var isDeleted = row.ReplyToIsDeleted ?? false;
        var autorEliminado = row.ReplyToSenderIsDeleted ?? false;

        return new MessageReplyDto
        {
            Id = row.ReplyToMessageId.Value,
            SenderDisplayName = autorEliminado
                ? UserPresentation.DeletedDisplayName
                : row.ReplyToDisplayName ?? string.Empty,
            Preview = isDeleted ? string.Empty : Clip(row.ReplyToContent ?? string.Empty),
            IsDeleted = isDeleted,
        };
    }

    private static string Clip(string content)
    {
        var flat = content.ReplaceLineEndings(" ").Trim();
        return flat.Length <= ReplyPreviewMaxLength
            ? flat
            : string.Concat(flat.AsSpan(0, ReplyPreviewMaxLength - 1), "…");
    }

    private static MessageDto MapMessage(MessageRow row, DateTime seenCutoff) => new()
    {
        Id = row.Id,
        ConversationId = row.ConversationId,
        // Un mensaje borrado no viaja con su texto, ni para quien lo mandó.
        Content = row.IsDeleted ? string.Empty : row.Content,
        CreatedAt = row.CreatedAt,
        IsEdited = row.EditedAt is not null,
        IsDeleted = row.IsDeleted,
        IsSeenByPeer = row.CreatedAt <= seenCutoff,
        ReplyTo = MapReply(row),
        Sender = new MessageAuthorDto
        {
            Id = row.SenderId,
            UserName = row.SenderIsDeleted ? string.Empty : row.SenderUserName,
            DisplayName = row.SenderIsDeleted
                ? UserPresentation.DeletedDisplayName
                : row.SenderDisplayName,
            ProfileImageUrl = row.SenderIsDeleted ? null : row.SenderProfileImageUrl,
            IsDeleted = row.SenderIsDeleted,
        },
    };

    private async Task<MessageDto> BuildMessageDtoAsync(Guid messageId, CancellationToken cancellationToken)
    {
        var row = await _dbContext.Messages
            .AsNoTracking()
            .Where(m => m.Id == messageId)
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
                m.IsDeleted,
                m.Sender.DeletedAt != null,
                m.ReplyToMessageId,
                m.ReplyToMessage!.Sender.DisplayName,
                m.ReplyToMessage!.Content,
                m.ReplyToMessage!.IsDeleted,
                m.ReplyToMessage!.Sender.DeletedAt != null))
            .FirstAsync(cancellationToken);

        // Acaba de salir: el otro todavía no pudo leerlo.
        return MapMessage(row, DateTime.MinValue);
    }
}
