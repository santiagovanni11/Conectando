using Conectando.Api.Extensions;
using Conectando.Api.Hubs;
using Conectando.Api.Hubs.Broadcasting;
using Conectando.Api.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Conectando.Api.Tests;

/// <summary>
/// Que las dependencias del hub estén registradas.
/// </summary>
/// <remarks>
/// El bug que esto cubre era invisible y silencioso: faltaba registrar la
/// clase que difunde los mensajes, así que SignalR no lograba activar el hub
/// y cerraba la conexión al instante, sin error en pantalla ni en el log. La
/// app entera andaba; solo el tiempo real estaba muerto.
///
/// Se consulta con <c>IsService</c> y no construyendo el servicio, porque
/// varias de estas dependencias llegan hasta la base de datos: el objetivo es
/// comprobar el registro, no levantar la infraestructura.
/// </remarks>
public class HubRegistrationTests
{
    private static ServiceProvider Build() => new ServiceCollection()
        .AddApplicationServices(new ConfigurationBuilder().Build())
        .BuildServiceProvider();

    private static bool IsRegistered<T>(ServiceProvider provider) =>
        provider.GetRequiredService<IServiceProviderIsService>().IsService(typeof(T));

    [Fact]
    public void El_broadcaster_que_difunde_los_mensajes_esta_registrado()
    {
        using var provider = Build();

        Assert.True(IsRegistered<ConversationBroadcaster>(provider));
    }

    [Fact]
    public void El_servicio_de_conversaciones_esta_registrado()
    {
        using var provider = Build();

        Assert.True(IsRegistered<IConversationService>(provider));
    }

    [Fact]
    public void El_almacenamiento_de_imagenes_esta_registrado()
    {
        using var provider = Build();

        Assert.True(IsRegistered<IMediaStorage>(provider));
    }
}
