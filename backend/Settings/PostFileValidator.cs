using System.IO;
using Conectando.Api.Exceptions;

namespace Conectando.Api.Settings;

/// <summary>
/// Decide si un archivo es una imagen aceptable.
/// </summary>
/// <remarks>
/// El tipo se saca de los primeros bytes del archivo, nunca del encabezado que
/// manda el navegador: ese lo escribe el cliente y se cambia en un segundo. La
/// extensión solo sirve para decidir qué se espera, y tiene que coincidir con lo
/// que el archivo realmente contiene. Si no, un `.jpg` que por dentro es un
/// script podría pasar como foto.
/// </remarks>
public static class PostFileValidator
{
    public const int MaxFileBytes = 8 * 1024 * 1024;

    /// <summary>
    /// Extensiones aceptadas y el tipo al que deben corresponder.
    /// ".jpeg" y ".jpe" son JPEG igual que ".jpg" y los usan cámaras y programas
    /// por todos lados. Antes no estaban, así que una foto JPEG legítima se
    /// rechazaba con un mensaje que decía que solo se admitían imágenes JPG: el
    /// usuario leía eso y de verdad había subido un JPG.
    /// </summary>
    private static readonly Dictionary<string, string> TiposPorExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".jpe"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp",
        [".gif"] = "image/gif",
    };

    /// <param name="contentType">
    /// Lo que declara el navegador. No se usa para decidir nada: es dato del
    /// cliente. Se deja en la firma para que quede a la vista que existe y que
    /// a propósito no se confía en él.
    /// </param>
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

        var extension = Path.GetExtension(fileName);
        if (!TiposPorExtension.TryGetValue(extension, out var expectedType))
        {
            throw new InvalidFileException(
                "Esa foto no tiene un formato admitido. Se aceptan JPG, PNG, WEBP y GIF. " +
                "Si es una foto de iPhone en formato HEIC, cambiá a Más compatible en " +
                "Ajustes → Cámara → Formatos.");
        }

        var detected = ImageSignature.Read(content);

        if (ImageSignature.IsHeic(content))
        {
            throw new InvalidFileException(
                "Las fotos de iPhone en formato HEIC todavía no se pueden subir. " +
                "En el iPhone: Ajustes → Cámara → Formatos → Más compatible.");
        }

        if (detected is null || !string.Equals(detected, expectedType, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidFileException("El archivo no es una imagen válida.");
        }

        return detected;
    }

}
