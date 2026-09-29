using System.Globalization;
using System.Text;

namespace Conectando.Api.Services;

public sealed record CommentCursor(DateTime CreatedAt, Guid Id);

public static class CommentCursorParser
{
    public static string Encode(DateTime createdAt, Guid id)
    {
        var value = $"{createdAt:O}|{id:N}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    }

    public static CommentCursor? Parse(string? cursor)
    {
        if (string.IsNullOrEmpty(cursor))
        {
            return null;
        }

        try
        {
            var bytes = Convert.FromBase64String(cursor);
            var value = Encoding.UTF8.GetString(bytes);
            var parts = value.Split('|', 2);

            if (parts.Length != 2)
            {
                return null;
            }

            if (!DateTime.TryParseExact(parts[0], "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var createdAt))
            {
                return null;
            }

            if (!Guid.TryParseExact(parts[1], "N", out var id))
            {
                return null;
            }

            return new CommentCursor(createdAt, id);
        }
        catch
        {
            return null;
        }
    }
}