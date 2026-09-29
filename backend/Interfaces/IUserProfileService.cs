using Conectando.Api.DTOs.Users;

namespace Conectando.Api.Interfaces;

public interface IUserProfileService
{
    Task<PrivateProfileDto> GetOwnProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PublicProfileDto> GetPublicProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PrivateProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<PrivateProfileDto> UploadProfileImageAsync(Guid userId, IFormFile file, CancellationToken cancellationToken = default);
}