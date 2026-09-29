namespace Conectando.Api.Settings;

/// <summary>
/// Reconoce el tipo de una imagen mirando sus primeros bytes.
/// </summary>
/// <remarks>
/// Va aparte de <see cref="PostFileValidator"/> porque es otra pregunta: la
/// política —qué se admite y con qué mensaje— es de uno, y la lectura de
/// firmas binarias, de otro. Juntas no dejaban lugar para crecer.
/// </remarks>
public static class ImageSignature
{
    private const int HeaderBytes = 16;

    /// <summary>
    /// Devuelve el tipo MIME real del archivo, o null si no es una imagen de
    /// las conocidas. El <paramref name="stream"/> queda donde estaba.
    /// </summary>
    public static string? Read(Stream content)
    {
        var bytes = new byte[HeaderBytes];
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

        if (slice.Length >= 8 && slice[..8].SequenceEqual(PngHeader))
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

    /// <summary>
    /// HEIC/HEIF usan un contenedor ISO-BMFF: "ftyp" en el byte 4 y la marca en
    /// el 8. Ningún formato aceptado empieza así, y conviene distinguirlo para
    /// poder explicar qué hacer: en un iPhone, decir "no es una imagen válida"
    /// es la respuesta menos útil que se puede dar.
    /// </summary>
    public static bool IsHeic(Stream content)
    {
        if (!content.CanSeek)
        {
            return false;
        }

        var position = content.Position;
        var header = new byte[12];
        var read = content.Read(header, 0, header.Length);
        content.Seek(position, SeekOrigin.Begin);

        if (read < 12 || !header[4..8].SequenceEqual("ftyp"u8))
        {
            return false;
        }

        var brand = System.Text.Encoding.ASCII.GetString(header, 8, 4);
        return HeicBrands.Contains(brand);
    }

    private static readonly byte[] PngHeader = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

    private static readonly HashSet<string> HeicBrands =
        new(StringComparer.Ordinal) { "heic", "heix", "hevc", "hevx", "mif1", "msf1" };
}
