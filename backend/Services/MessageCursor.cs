using System.Globalization;
using System.Text;

namespace Conectando.Api.Services;

internal sealed record MessageCursor(DateTime CreatedAt, Guid Id);

/// <summary>Fila aplanada de mensaje, con los datos del autor ya resueltos.</summary>
internal sealed record MessageRow(
    Guid Id,
    Guid ConversationId,
    string Content,
    DateTime CreatedAt,
    Guid SenderId,
    string SenderUserName,
    string SenderDisplayName,
    string? SenderProfileImageUrl,
    DateTime? EditedAt,
    bool IsDeleted);

internal sealed record PeerMessageStamp(Guid ConversationId, DateTime CreatedAt);

/// <summary>Par (conversación, id del último mensaje) para armar la lista.</summary>
internal sealed record LastMessageStamp(Guid ConversationId, Guid LastMessageId);

/// <summary>Miembro de una conversación con los datos de su usuario ya resueltos.</summary>
internal sealed record ConversationMemberRow(
    Guid ConversationId,
    Guid UserId,
    string UserName,
    string DisplayName,
    string? ProfileImageUrl,
    DateTime? LastReadAt);

/// <summary>Igual que el anterior pero sin la conversación, para consultas de un solo par.</summary>
internal sealed record PeerRow(
    Guid UserId,
    string UserName,
    string DisplayName,
    string? ProfileImageUrl);

/// <summary>
/// Postgres guarda las fechas con precisión de microsegundos y .NET con
/// 100 nanosegundos. Si no se recorta, el valor guardado nunca coincide
/// exactamente con el que quedó en memoria.
/// </summary>
internal static class DatabaseTime
{
    public static DateTime UtcNow()
    {
        // Se lee el reloj una sola vez: llamarlo dos veces cruzaría el
        // límite de tick y el recorte quedaría mal.
        var now = DateTime.UtcNow;
        return now.AddTicks(-(now.Ticks % 10));
    }
}

internal static class MessageCursorParser
{
    public static string Encode(DateTime createdAt, Guid id)
    {
        var value = $"{createdAt:O}|{id:N}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    }

    public static MessageCursor? Parse(string? cursor)
    {
        if (string.IsNullOrEmpty(cursor)) return null;

        try
        {
            var value = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var separatorIndex = value.IndexOf('|');
            if (separatorIndex < 0) return null;

            var createdAtString = value[..separatorIndex];
            var idString = value[(separatorIndex + 1)..];

            if (!DateTime.TryParse(createdAtString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var createdAt))
            {
                return null;
            }

            if (!Guid.TryParseExact(idString, "N", out var id)) return null;

            return new MessageCursor(createdAt, id);
        }
        catch (FormatException)
        {
            return null;
        }
    }
}