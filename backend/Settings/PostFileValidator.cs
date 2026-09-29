using System.IO;
using Conectando.Api.Exceptions;

namespace Conectando.Api.Settings;

public static class PostFileValidator
{
    public const int MaxFileBytes = 8 * 1024 * 1024;

    private const string JpegExtension = ".jpg";
    private const string PngExtension = ".png";
    private const string WebpExtension = ".webp";
    private const string GifExtension = ".gif";

    public static string Validate(string fileName, string contentType, long length, Stream content)
    {
        if (length <= 0)
        {
            throw new InvalidFileException("El archivo está vacío.");
        }

        if (length > MaxFileBytes)
        {
            throw new InvalidFileException("Cada foto puede pesar hasta 8 MB.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var expectedType = extension switch
        {
            JpegExtension => "image/jpeg",
            PngExtension => "image/png",
            WebpExtension => "image/webp",
            GifExtension => "image/gif",
            _ => throw new InvalidFileException("Solo se admiten imágenes JPG, PNG, WEBP o GIF."),
        };

        var detected = Sniff(content);
        if (detected is null || !string.Equals(detected, expectedType, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidFileException("El archivo no es una imagen válida.");
        }

        return detected;
    }

    private static string? Sniff(Stream content)
    {
        var bytes = new byte[16];
        var initial = content.CanSeek ? content.Position : 0;
        var read = 0;
        while (read < bytes.Length)
        {
            var count = content.Read(bytes, read, bytes.Length - read);
            if (count == 0)
            {
                break;
            }

            read += count;
        }

        if (content.CanSeek)
        {
            content.Seek(initial, SeekOrigin.Begin);
        }

        var slice = bytes.AsSpan(0, read);

        if (slice.Length >= 8 && slice[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
        {
            return "image/png";
        }

        if (slice.Length >= 3 && slice[0] == 0xFF && slice[1] == 0xD8 && slice[2] == 0xFF)
        {
            return "image/jpeg";
        }

        if (slice.Length >= 12 && slice[..4].SequenceEqual("RIFF"u8) && slice[8..12].SequenceEqual("WEBP"u8))
        {
            return "image/webp";
        }

        if (slice.Length >= 6 && (slice[..6].SequenceEqual("GIF87a"u8) || slice[..6].SequenceEqual("GIF89a"u8)))
        {
            return "image/gif";
        }

        return null;
    }
}