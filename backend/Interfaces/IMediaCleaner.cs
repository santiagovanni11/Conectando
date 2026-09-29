namespace Conectando.Api.Interfaces;

/// <summary>
/// Borra un archivo del almacenamiento, sin propagar el fallo.
/// </summary>
/// <remarks>
/// Existe porque "borrar en Cloudinary" es una limpieza, nunca la operación
/// que el usuario pidió. Si el borrado falla, la fila igual tiene que quedar
/// borrada: al revés, el usuario queda con un registro que apunta a un archivo
/// que quizá no existe, y el error le llega como si su operación hubiera
/// fallado cuando en realidad se guardó bien.
/// </remarks>
public interface IMediaCleaner
{
    Task TryDeleteAsync(string? publicId, CancellationToken cancellationToken = default);
}
