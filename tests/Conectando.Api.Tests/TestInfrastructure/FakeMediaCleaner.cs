using Conectando.Api.Interfaces;

namespace Conectando.Api.Tests.TestInfrastructure;

/// <summary>
/// Almacenamiento falso que anota qué se le pidió borrar.
///
/// Existe para poder comprobar que al dar de baja una cuenta se limpian los
/// archivos, y no solo las filas. La fila desaparece igual en los dos casos,
/// así que sin esto un archivo olvidado en Cloudinary pasaría inadvertido:
/// la base quedaría limpia y el archivo seguiría existiendo y costeando.
///
/// <para>
/// No falla nunca a propósito. El limpiador de verdad absorbe el error, y un
/// test que lo fallara se detendría en la mitad y no probaría lo que importa.
/// </summary>
public sealed class FakeMediaCleaner : IMediaCleaner
{
    public List<string> Borrados { get; } = [];

    public Task TryDeleteAsync(string? publicId, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(publicId))
        {
            Borrados.Add(publicId);
        }

        return Task.CompletedTask;
    }
}