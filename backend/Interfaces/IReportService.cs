using Conectando.Api.DTOs.Social;

namespace Conectando.Api.Interfaces;

public interface IReportService
{
    /// <summary>Registra una denuncia. Devuelve la existente si ya había una igual.</summary>
    Task<ReportDto> CreateAsync(Guid reporterId, Guid targetUserId, string reason, string? details, CancellationToken cancellationToken = default);

    /// <summary>Las denuncias que hizo el usuario. Nadie ve las de otros.</summary>
    Task<List<ReportDto>> GetOwnAsync(Guid reporterId, CancellationToken cancellationToken = default);
}