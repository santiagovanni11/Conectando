namespace Conectando.Api.Models;

/// <summary>
/// Motivos por los que se puede denunciar. Se guardan como texto para que
/// agregar uno nuevo no requiera migración: el enum es la lista cerrada de
/// lo que la interfaz ofrece, no la forma de almacenarlo.
/// </summary>
public static class ReportReasons
{
    public const string Spam = "spam";
    public const string Harassment = "harassment";
    public const string HateSpeech = "hate_speech";
    public const string Violence = "violence";
    public const string Nudity = "nudity";
    public const string FalseIdentity = "false_identity";
    public const string Other = "other";
}

/// <summary>Denuncia que un usuario hace sobre otro.</summary>
public class Report
{
    public Guid Id { get; set; }

    /// <summary>Quien denuncia. Es el único que puede ver la denuncia.</summary>
    public Guid ReporterId { get; set; }

    /// <summary>A quién se denuncia.</summary>
    public Guid TargetUserId { get; set; }

    public string Reason { get; set; } = string.Empty;

    /// <summary>Comentario opcional del denunciante.</summary>
    public string? Details { get; set; }

    public DateTime CreatedAt { get; set; }
}