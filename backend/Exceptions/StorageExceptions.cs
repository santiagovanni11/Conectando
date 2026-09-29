namespace Conectando.Api.Exceptions;

/// <summary>
/// Se le acabó el espacio al usuario.
/// </summary>
/// <remarks>
/// 507 y no 400 a propósito: el 400 ya lo usa "mandaste demasiados archivos",
/// que se corrige ajustando lo que mandás. Lleno de espacio se corrige
/// borrando algo, y el frontend tiene que poder ofrecer exactamente eso.
/// </remarks>
public class StorageQuotaExceededException : ApiException
{
    public StorageQuotaExceededException(long attemptedBytes, long limitBytes)
        : base(
            StatusCodes.Status507InsufficientStorage,
            $"Te quedaste sin espacio para archivos: ocupa {Human(attemptedBytes)} de {Human(limitBytes)}. " +
            "Borrá una publicación o una foto para poder seguir subiendo.")
    {
    }

    private static string Human(long bytes)
    {
        var megabytes = bytes / (1024d * 1024d);
        return megabytes >= 1 ? $"{megabytes:0.#} MB" : $"{bytes / 1024d:0.#} KB";
    }
}
