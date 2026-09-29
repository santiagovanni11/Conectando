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
            // El encuadre era de la foto anterior: con una imagen nueva no
            // significa nada y dejaría la foto corrida.
            user.ResetAvatarFraming();
        }
        else
        {
            // Null significa "no lo toqué". Sin este if, guardar la
            // biografía volvería el avatar al centro.
            ApplyAvatarFraming(user, request);
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
        ProfileImageZoom = user.ProfileImageZoom,
        ProfileImageOffsetX = user.ProfileImageOffsetX,
        ProfileImageOffsetY = user.ProfileImageOffsetY,
        IsPrivate = user.IsPrivate,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
    };

    /// <summary>
    /// Aplica el encuadre que venga completo. Los tres campos van juntos: si
    /// llega uno solo, se deja el encuadre como estaba en vez de mezclar un
    /// desplazamiento nuevo con un acercamiento viejo.
    /// </summary>
    private static void ApplyAvatarFraming(AppUser user, UpdateProfileRequest request)
    {
        if (request.ProfileImageZoom is null || request.ProfileImageOffsetX is null || request.ProfileImageOffsetY is null)
        {
            return;
        }

        user.ProfileImageZoom = request.ProfileImageZoom.Value;
        user.ProfileImageOffsetX = request.ProfileImageOffsetX.Value;
        user.ProfileImageOffsetY = request.ProfileImageOffsetY.Value;
    }

    private static string? ToNullOrTrimmed(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
