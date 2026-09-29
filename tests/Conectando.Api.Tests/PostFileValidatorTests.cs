using Conectando.Api.Exceptions;
using Conectando.Api.Settings;

namespace Conectando.Api.Tests;

public class PostFileValidatorTests
{
    private static readonly byte[] PngBytes =
    {
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
    };

    [Fact]
    public void Validate_ValidPng_ReturnsType()
    {
        using var stream = new MemoryStream(PngBytes);

        var type = PostFileValidator.Validate("foto.png", "image/png", PngBytes.Length, stream);

        Assert.Equal("image/png", type);
    }

    [Fact]
    public void Validate_ValidJpeg_ReturnsType()
    {
        using var stream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 });

        var type = PostFileValidator.Validate("foto.jpg", "image/jpeg", 6, stream);

        Assert.Equal("image/jpeg", type);
    }

    [Fact]
    public void Validate_ExtensionMismatch_Throws()
    {
        using var stream = new MemoryStream(PngBytes);

        Assert.Throws<InvalidFileException>(() => PostFileValidator.Validate("foto.jpg", "image/jpeg", PngBytes.Length, stream));
    }

    [Fact]
    public void Validate_SpoofedSignature_Throws()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("no es una imagen"));

        Assert.Throws<InvalidFileException>(() => PostFileValidator.Validate("foto.png", "image/png", stream.Length, stream));
    }

    [Fact]
    public void Validate_UnknownExtension_Throws()
    {
        using var stream = new MemoryStream(PngBytes);

        Assert.Throws<InvalidFileException>(() => PostFileValidator.Validate("foto.txt", "text/plain", PngBytes.Length, stream));
    }

    [Fact]
    public void Validate_EmptyFile_Throws()
    {
        using var stream = new MemoryStream();

        Assert.Throws<InvalidFileException>(() => PostFileValidator.Validate("foto.png", "image/png", 0, stream));
    }

    [Fact]
    public void Validate_TooLargeFile_Throws()
    {
        using var stream = new MemoryStream(PngBytes);
        const long oversized = PostFileValidator.MaxFileBytes + 1;

        Assert.Throws<InvalidFileException>(() => PostFileValidator.Validate("foto.png", "image/png", oversized, stream));
    }

    // ── Extensiones alternativas ──────────────────────────────────────
    // El bug que estos tests cubren: ".jpeg" es un JPEG igual de válido que
    // ".jpg" y lo usan muchos programas y cámaras. Estaba en la lista negra
    // de hecho, así que un archivo JPEG legítimo se rechazaba diciendo que
    // solo se admitían imágenes JPG. El mensaje de error era el que confundía:
    // el usuario leía "solo JPG" y había subido un JPG.

    [Theory]
    [InlineData("foto.jpeg")]
    [InlineData("foto.JPEG")]
    [InlineData("foto.Jpeg")]
    [InlineData("IMAGEN.JPEG")]
    [InlineData("foto.jpe")]
    public void Validate_JpegConOtraExtension_Acepta(string name)
    {
        using var stream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 });

        var type = PostFileValidator.Validate(name, "image/jpeg", 6, stream);

        Assert.Equal("image/jpeg", type);
    }

    [Fact]
    public void Validate_JpegConContentTypeNoEstandar_Acepta()
    {
        // Algunos navegadores y herramientas mandan "image/jpg", que no es
        // un tipo válido. El tipo lo decide el contenido, no el encabezado.
        using var stream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 });

        var type = PostFileValidator.Validate("foto.jpg", "image/jpg", 6, stream);

        Assert.Equal("image/jpeg", type);
    }

    [Fact]
    public void Validate_Heic_ExplicaComoArreglarlo()
    {
        // La foto del iPhone llega como HEIC. Decirle "no es una imagen válida"
        // deja al usuario sin saber qué hacer; el mensaje tiene que decirlo.
        using var stream = new MemoryStream(Heic("heic"));

        var error = Assert.Throws<InvalidFileException>(() =>
            PostFileValidator.Validate("foto.heic", "image/heic", 12, stream));

        Assert.Contains("HEIC", error.Message);
        Assert.Contains("Más compatible", error.Message);
    }

    [Fact]
    public void Validate_ExtensionDesconocida_InformaDeHeic()
    {
        // Aunque la extensión ya no sirva, el mensaje debe orientar al que
        // tiene un iPhone: es el caso más común y el que más confunde.
        using var stream = new MemoryStream(Heic("heic"));

        var error = Assert.Throws<InvalidFileException>(() =>
            PostFileValidator.Validate("foto.heic", "image/heic", 12, stream));

        Assert.Contains("HEIC", error.Message);
    }

    /// <summary>Cabecera mínima de un archivo ISO-BMFF con la marca dada.</summary>
    private static byte[] Heic(string brand) => [0, 0, 0, 24, (byte)'f', (byte)'t', (byte)'y', (byte)'p', .. brand.Select(c => (byte)c), .. new byte[4]];
}
