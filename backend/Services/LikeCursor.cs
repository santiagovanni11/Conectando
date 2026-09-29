using System.Globalization;
using System.Text;

namespace Conectando.Api.Services;

internal sealed record LikeCursor(DateTime CreatedAt, Guid UserId);

internal static class LikeCursorParser
{
    public static string Encode(DateTime createdAt, Guid userId)
    {
        var value = $"{createdAt:O}|{userId:N}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    }

    public static LikeCursor? Parse(string? cursor)
    {
        if (string.IsNullOrEmpty(cursor))
        {
            return null;
        }

        try
        {
            var bytes = Convert.FromBase64String(cursor);
            var value = Encoding.UTF8.GetString(bytes);
            var separatorIndex = value.IndexOf('|');

            if (separatorIndex < 0)
            {
                return null;
            }

            var createdAtString = value[..separatorIndex];
            var userIdString = value[(separatorIndex + 1)..];

            if (!DateTime.TryParse(createdAtString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var createdAt))
            {
                return null;
            }

            if (!Guid.TryParse(userIdString, out var userId))
            {
                return null;
            }

            return new LikeCursor(createdAt, userId);
        }
        catch
        {
            return null;
        }
    }
}