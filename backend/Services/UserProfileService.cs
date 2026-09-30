using Conectando.Api.Data;
using Conectando.Api.DTOs.Users;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Perfil propio: lectura, edición y foto de avatar.
/// </summary>
/// <remarks>
/// En parciales por responsabilidad, igual que el resto de servicios que
/// pasaron de 150 líneas. Esta parte solo lee.
/// </remarks>
public partial class UserProfileService : IUserProfileService
{
    private readonly ConectandoDbContext dbContext;
    private readonly IMediaStorage storage;
    private readonly IMediaCleaner cleaner;
    private readonly IStorageQuotaService quota;

    public UserProfileService(
        ConectandoDbContext dbContext,
        IMediaStorage storage,
        IMediaCleaner cleaner,
        IStorageQuotaService quota)
    {
        this.dbContext = dbContext;
        this.storage = storage;
        this.cleaner = cleaner;
        this.quota = quota;
    }

    public async Task<PrivateProfileDto> GetOwnProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new PrivateProfileDto
            {
                Id = u.Id,
                UserName = u.UserName,
                DisplayName = u.DisplayName,
                Email = u.Email,
                Bio = u.Bio,
                ProfileImageUrl = u.ProfileImageUrl,
                ProfileImageZoom = u.ProfileImageZoom,
                ProfileImageOffsetX = u.ProfileImageOffsetX,
                ProfileImageOffsetY = u.ProfileImageOffsetY,
                IsPrivate = u.IsPrivate,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
            })
            .SingleOrDefaultAsync(cancellationToken);

        return profile ?? throw new UserNotFoundException();
    }

    public async Task<PublicProfileDto> GetPublicProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId && u.DeletedAt == null)
            .Select(u => new PublicProfileDto
            {
                Id = u.Id,
                UserName = u.UserName,
                DisplayName = u.DisplayName,
                Bio = u.Bio,
                ProfileImageUrl = u.ProfileImageUrl,
                ProfileImageZoom = u.ProfileImageZoom,
                ProfileImageOffsetX = u.ProfileImageOffsetX,
                ProfileImageOffsetY = u.ProfileImageOffsetY,
                IsPrivate = u.IsPrivate,
                CreatedAt = u.CreatedAt,
            })
            .SingleOrDefaultAsync(cancellationToken);

        return profile ?? throw new UserNotFoundException();
    }
}
