using Conectando.Api.DTOs.Users;
using Conectando.Api.Exceptions;
using Conectando.Api.Settings;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Foto de perfil.
/// </summary>
public partial class UserProfileService
{
    public async Task<PrivateProfileDto> UploadProfileImageAsync(Guid userId, IFormFile file, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new UserNotFoundException();
        }

        // Se consulta antes de subir. acá se cuenta el avatar viejo que sigue
        // ocupando, así que el margen es una imagen más justo; con el orden
        // inverso (borrar primero,=subir después) un corte de luz dejaría al
        // usuario sin avatar y con el archivo viejo ya destruido.
        await quota.EnsureCanStoreAsync(userId, file.Length, cancellationToken);

        await using var stream = file.OpenReadStream();
        var contentType = PostFileValidator.Validate(file.FileName, file.ContentType, file.Length, stream);
        var stored = await storage.UploadAsync(stream, file.FileName, contentType, cancellationToken);

        var previousPublicId = user.ProfileImagePublicId;

        user.ProfileImageUrl = stored.Url;
        user.ProfileImagePublicId = stored.PublicId;
        user.ProfileImageSizeBytes = file.Length;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        // Antes esto no existía: cada cambio de avatar dejaba la imagen vieja
        // en Cloudinary, sin nada que la referencie y pagando por siempre.
        await cleaner.TryDeleteAsync(previousPublicId, cancellationToken);

        return MapToPrivateProfile(user);
    }
}
