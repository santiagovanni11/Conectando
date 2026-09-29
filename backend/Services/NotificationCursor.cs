using System.Globalization;
using System.Text;

namespace Conectando.Api.Services;

internal sealed record NotificationRow(
    Guid Id,
    Models.NotificationType Type,
    DateTime CreatedAt,
    DateTime? ReadAt,
    Guid? PostId,
    Guid ActorId,
    string ActorUserName,
    string ActorDisplayName,
    string? ActorProfileImageUrl);

internal sealed record NotificationCursor(DateTime CreatedAt, Guid Id);

internal static class NotificationCursorParser
{
    public static string Encode(DateTime createdAt, Guid id)
    {
        var value = $"{createdAt:O}|{id:N}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    }

    public static NotificationCursor? Parse(string? cursor)
    {
        if (string.IsNullOrEmpty(cursor)) return null;

        try
        {
            var bytes = Convert.FromBase64String(cursor);
            var value = Encoding.UTF8.GetString(bytes);
            var separatorIndex = value.IndexOf('|');
            if (separatorIndex < 0) return null;

            var createdAtString = value[..separatorIndex];
            var idString = value[(separatorIndex + 1)..];

            if (!DateTime.TryParse(createdAtString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var createdAt))
            {
                return null;
            }

            if (!Guid.TryParseExact(idString, "N", out var id)) return null;

            return new NotificationCursor(createdAt, id);
        }
        catch (FormatException)
        {
            return null;
        }
    }
}