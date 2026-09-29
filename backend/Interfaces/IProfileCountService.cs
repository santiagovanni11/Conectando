using Conectando.Api.DTOs.Users;

namespace Conectando.Api.Interfaces;

public interface IProfileCountService
{
    /// <summary>Conteos del perfil ya descontando a quien el viewer bloqueó.</summary>
    Task<ProfileCountsDto> GetCountsAsync(Guid userId, Guid viewerId, CancellationToken cancellationToken = default);
}