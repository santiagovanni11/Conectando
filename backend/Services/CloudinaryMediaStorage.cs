using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Settings;

namespace Conectando.Api.Services;

public class CloudinaryMediaStorage : IMediaStorage
{
    private readonly Cloudinary? _cloudinary;
    private readonly ILogger<CloudinaryMediaStorage> _logger;

    public CloudinaryMediaStorage(IConfiguration configuration, ILogger<CloudinaryMediaStorage> logger)
    {
        _logger = logger;
        var settings = configuration.GetSection(CloudinarySettings.SectionName).Get<CloudinarySettings>();
        if (settings is not null && settings.IsConfigured)
        {
            _cloudinary = new Cloudinary(new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret));
        }
    }

    public async Task<StoredMedia> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        if (_cloudinary is null)
        {
            throw new MediaStorageNotConfiguredException();
        }

        var publicId = $"posts/{Guid.NewGuid():N}";
        var parameters = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            PublicId = publicId,
            Overwrite = false,
        };

        var result = await _cloudinary.UploadAsync(parameters, cancellationToken);
        if (result.Error is not null || result.SecureUrl is null)
        {
            // Cloudinary sí dice por qué rechazó la subida, y ese motivo se
            // estaba tirando a la basura. El único síntoma era un error
            // genérico, sin forma de distinguir una clave mal puesta de una
            // cuenta sin activar o un límite de la cuota.
            _logger.LogError(
                "Cloudinary rechazó la subida de {FileName}: {Message}",
                fileName,
                result.Error?.Message);

            throw new MediaStorageException();
        }

        return new StoredMedia(result.SecureUrl.AbsoluteUri, publicId);
    }

    public async Task DeleteAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (_cloudinary is null || string.IsNullOrWhiteSpace(publicId))
        {
            return;
        }

        await _cloudinary.DestroyAsync(new DeletionParams(publicId));
    }
}