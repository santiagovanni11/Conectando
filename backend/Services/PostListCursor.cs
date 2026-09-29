using System.Buffers.Text;
using System.Globalization;
using Conectando.Api.Models;

namespace Conectando.Api.Services;

public readonly record struct PostListCursor(DateTime CreatedAt, Guid Id)
{
    public static PostListCursor? TryDecode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            var bytes = Base64Url.DecodeFromChars(value);
            var text = System.Text.Encoding.UTF8.GetString(bytes);
            var parts = text.Split('|', 2);
            if (parts.Length != 2)
            {
                return null;
            }

            var created = DateTime.ParseExact(parts[0], "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            return new PostListCursor(created, Guid.Parse(parts[1]));
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static string Encode(Post post) =>
        Base64Url.EncodeToString(System.Text.Encoding.UTF8.GetBytes($"{post.CreatedAt.ToString("O", CultureInfo.InvariantCulture)}|{post.Id:D}"));
}