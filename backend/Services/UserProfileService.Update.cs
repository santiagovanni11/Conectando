using Conectando.Api.Data;
using Conectando.Api.DTOs.Users;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Edición de los datos del perfil.
/// </summary>
public partial class UserProfileService
{
    public async Task<PrivateProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new UserNotFoundException();
        }

        // Se comparan contra lo que había antes de pisar el campo, o el
        // cambio nunca se detecta.
        var previousPublicId = user.ProfileImagePublicId;
        var previousUrl = user.ProfileImageUrl;
        var nextImage = ToNullOrTrimmed(request.ProfileImageUrl);

        user.DisplayName = request.DisplayName.Trim();
        user.Bio = ToNullOrTrimmed(request.Bio);
        user.ProfileImageUrl = nextImage;
        user.IsPrivate = request.IsPrivate;
        user.UpdatedAt = DateTime.UtcNow;

        var imageChanged = nextImage != previousUrl;
        if (imageChanged)
        {
            user.ProfileImagePublicId = null;
            user.ProfileImageSizeBytes = 0;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Recién con la fila ya consistente se borra el archivo anterior: si
        // fallara el guardado, no quedaría un avatar apuntando a un publicId
        // que ya no existe.
        if (imageChanged)
        {
            await cleaner.TryDeleteAsync(previousPublicId, cancellationToken);
        }

        return MapToPrivateProfile(user);
    }

    private static PrivateProfileDto MapToPrivateProfile(AppUser user) => new()
    {
        Id = user.Id,
        UserName = user.UserName,
        DisplayName = user.DisplayName,
        Email = user.Email,
        Bio = user.Bio,
        ProfileImageUrl = user.ProfileImageUrl,
        IsPrivate = user.IsPrivate,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
    };

    private static string? ToNullOrTrimmed(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
