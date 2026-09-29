using System.Reflection;
using Conectando.Api.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Conectando.Api.Tests;

/// <summary>
/// Seguridad de los hubs.
/// </summary>
/// <remarks>
/// Cubre todos los hubs, no solo el de mensajes, para que un hub nuevo
/// nazca protegido sin que nadie tenga que acordarse.
/// </remarks>
public class HubAuthorizationTests
{
    public static TheoryData<Type> ConcreteHubs()
    {
        var data = new TheoryData<Type>();

        foreach (var type in typeof(MessageHub).Assembly.GetTypes()
                     .Where(t => typeof(Hub).IsAssignableFrom(t) && !t.IsAbstract))
        {
            data.Add(type);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(ConcreteHubs))]
    public void Hub_RequiresAuthentication(Type hubType)
    {
        // Regresion: al extraer la base ConectandoHub se perdio el
        // [Authorize] de MessageHub. Como los atributos NO se heredan de
        // una clase base, el hub quedo abierto: cualquier conexion
        // anonima podia entrar al canal de mensajes.
        var isAuthorized = hubType
            .GetCustomAttribute<AuthorizeAttribute>(inherit: false) is not null;

        Assert.True(isAuthorized, $"{hubType.Name} no exige autenticacion");
    }
}
