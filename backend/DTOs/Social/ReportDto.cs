namespace Conectando.Api.DTOs.Social;

public class CreateReportRequest
{
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
}

public class ReportDto
{
    public Guid Id { get; init; }
    public Guid TargetUserId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Details { get; init; }
    public DateTime CreatedAt { get; init; }
}