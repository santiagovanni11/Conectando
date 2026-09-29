namespace Conectando.Api.Interfaces;

/// <summary>
/// Controla cuánto espacio lleva usado un usuario.
/// </summary>
/// <remarks>
/// Vive aparte de los servicios de medios a propósito. Fotos de
/// publicaciones y foto de perfil son dos caminos distintos, pero la cuenta
/// es la misma: si cada uno hiciese su propio cálculo, pasarían distintos y
/// el total no cerraría con lo que dice el panel del usuario.
/// </remarks>
public interface IStorageQuotaService
{
    /// <summary>Bytes ocupados hoy: fotos de publicaciones más foto de perfil.</summary>
    Task<long> GetUsageAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Falla si guardar <paramref name="additionalBytes"/> más excede el tope.
    /// Se consulta antes de subir, no después.
    /// </summary>
    Task EnsureCanStoreAsync(Guid userId, long additionalBytes, CancellationToken cancellationToken = default);
}
