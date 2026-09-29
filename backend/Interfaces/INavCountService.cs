using Conectando.Api.DTOs.Counters;

namespace Conectando.Api.Interfaces;

/// <summary>Cantidades de aviso para la navegación.</summary>
public interface INavCountService
{
    Task<NavCountsDto> GetCountsAsync(Guid userId, CancellationToken cancellationToken = default);
}