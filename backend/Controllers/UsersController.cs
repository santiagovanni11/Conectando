using Conectando.Api.DTOs.Users;
using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Conectando.Api.RateLimiting;
using Conectando.Api.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Conectando.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(
    IUserProfileService profiles,
    IUserDiscoveryService discovery,
    IUserConnectionsService connections,
    IProfileCountService counts) : ControllerBase
{
    [HttpGet("me/profile")]
    public async Task<ActionResult<PrivateProfileDto>> GetOwnProfile(CancellationToken cancellationToken)
    {
        var profile = await profiles.GetOwnProfileAsync(User.GetUserId(), cancellationToken);
        return Ok(profile);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PublicProfileDto>> GetPublicProfile(Guid id, CancellationToken cancellationToken)
    {
        var profile = await profiles.GetPublicProfileAsync(id, cancellationToken);
        return Ok(profile);
    }

    /// <summary>
    /// Conteos del perfil de otro usuario. Van en un endpoint aparte porque
    /// dependen de quién mira: el perfil base no sabe si el que consulta
    /// bloqueó a alguno de los seguidores.
    /// </summary>
    [HttpGet("{id:guid}/counts")]
    public async Task<ActionResult<ProfileCountsDto>> GetPublicCounts(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await counts.GetCountsAsync(id, User.GetUserId(), cancellationToken));
    }

    [HttpPut("me/profile")]
    public async Task<ActionResult<PrivateProfileDto>> UpdateProfile(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await profiles.UpdateProfileAsync(User.GetUserId(), request, cancellationToken);
        return Ok(profile);
    }

    [HttpPost("me/profile-image")]
    [EnableRateLimiting(RateLimitPolicies.Uploads)]
    [RequestSizeLimit(PostConstants.MaxUploadRequestBodyBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = PostConstants.MaxUploadRequestBodyBytes)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PrivateProfileDto>> UploadProfileImage(
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        var profile = await profiles.UploadProfileImageAsync(User.GetUserId(), file, cancellationToken);
        return Ok(profile);
    }
}