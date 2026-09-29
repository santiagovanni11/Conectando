using Conectando.Api.Data;
using Conectando.Api.DTOs.Social;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Conectando.Api.Settings;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Denuncias de usuarios.
///
/// No hay panel de moderación todavía: las帷幕 quedan registradas y se
/// revisan a mano. Esa decisión evita publicar datos de otros usuarios
/// antes de que exista quién los mire.
/// </summary>
public class ReportService(ConectandoDbContext dbContext) : IReportService
{
    private static readonly HashSet<string> AllowedReasons =
    [
        ReportReasons.Spam,
        ReportReasons.Harassment,
        ReportReasons.HateSpeech,
        ReportReasons.Violence,
        ReportReasons.Nudity,
        ReportReasons.FalseIdentity,
        ReportReasons.Other,
    ];

    private readonly ConectandoDbContext _dbContext = dbContext;

    public async Task<ReportDto> CreateAsync(
        Guid reporterId,
        Guid targetUserId,
        string reason,
        string? details,
        CancellationToken cancellationToken = default)
    {
        if (reporterId == targetUserId) throw new SelfActionException();

        var normalized = reason?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!AllowedReasons.Contains(normalized)) throw new InvalidReportReasonException();

        var trimmedDetails = details?.Trim();
        if (trimmedDetails?.Length > ReportLimits.MaxDetails)
        {
            throw new ReportDetailsTooLongException(ReportLimits.MaxDetails);
        }

        if (!await _dbContext.Users.AnyAsync(u => u.Id == targetUserId, cancellationToken))
        {
            throw new UserNotFoundException();
        }

        // Denunciar dos veces a la misma persona actualiza la denuncia en
        // lugar de duplicarla: así el índice único no explota y se conserva
        // la información más reciente.
        var existing = await _dbContext.Reports.FirstOrDefaultAsync(
            r => r.ReporterId == reporterId && r.TargetUserId == targetUserId,
            cancellationToken);

        if (existing is not null)
        {
            existing.Reason = normalized;
            existing.Details = string.IsNullOrEmpty(trimmedDetails) ? null : trimmedDetails;
            existing.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new Report
            {
                ReporterId = reporterId,
                TargetUserId = targetUserId,
                Reason = normalized,
                Details = string.IsNullOrEmpty(trimmedDetails) ? null : trimmedDetails,
                CreatedAt = DateTime.UtcNow,
            };
            _dbContext.Reports.Add(existing);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(existing);
    }

    public async Task<List<ReportDto>> GetOwnAsync(
        Guid reporterId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reports
            .AsNoTracking()
            .Where(r => r.ReporterId == reporterId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => ToDto(r))
            .ToListAsync(cancellationToken);
    }

    private static ReportDto ToDto(Report report) => new()
    {
        Id = report.Id,
        TargetUserId = report.TargetUserId,
        Reason = report.Reason,
        Details = report.Details,
        CreatedAt = report.CreatedAt,
    };
}