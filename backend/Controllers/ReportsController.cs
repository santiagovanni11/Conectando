using Conectando.Api.DTOs.Social;
using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Conectando.Api.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Conectando.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public class ReportsController(IReportService reportService) : ControllerBase
{
    private readonly IReportService _reportService = reportService;

    /// <summary>Denuncia a un usuario. El motivo tiene que estar en la lista cerrada.</summary>
    [HttpPost("{userId:guid}")]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    public async Task<ActionResult<ReportDto>> Create(
        Guid userId,
        CreateReportRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.CreateAsync(
            User.GetUserId(), userId, request.Reason, request.Details, cancellationToken);

        return Ok(result);
    }

    /// <summary>Las denuncias del usuario actual.</summary>
    [HttpGet]
    public async Task<ActionResult<List<ReportDto>>> GetOwn(CancellationToken cancellationToken)
    {
        return Ok(await _reportService.GetOwnAsync(User.GetUserId(), cancellationToken));
    }
}