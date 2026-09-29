namespace Conectando.Api.Settings;

/// <summary>
/// Tope de espacio por usuario.
/// </summary>
/// <remarks>
/// Va en configuración y no como constante para poder moverlo por entorno
/// sin recompilar. El límite existe para proteger la factura de Cloudinary:
/// sin él, el único freno son los 8 archivos por publicación, y un usuario
/// podría publicar sin límite. El default es un número redondo que no
/// molesta a nadie en uso normal.
/// </remarks>
public class StorageQuotaSettings
{
    public const string SectionName = "StorageQuota";

    public long MaxBytesPerUser { get; init; } = 200L * 1024 * 1024;
}
