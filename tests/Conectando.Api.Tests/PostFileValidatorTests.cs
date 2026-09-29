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
}