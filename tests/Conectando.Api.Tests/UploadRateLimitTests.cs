using System.Reflection;
using Conectando.Api.Controllers;
using Conectando.Api.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Conectando.Api.Tests;

/// <summary>
/// Los atributos de rate limit en las subidas.
/// </summary>
/// <remarks>
/// No se prueba el limitador en sí (eso es del framework), sino que el
/// atributo siga puesto. Es lo que se pierde en silencio: alguien mueve un
/// endpoint, copia el <c>[HttpPost]</c> y se olvida el
/// <c>[EnableRateLimiting]</c>. El código sigue siendo correcto, solo que
/// desprotegido, y ningún test falla.
/// </remarks>
public class UploadRateLimitTests
{
    [Theory]
    [InlineData(typeof(PostMediaController), "UploadMedia")]
    [InlineData(typeof(PostMediaController), "DeleteMedia")]
    [InlineData(typeof(UsersController), "UploadProfileImage")]
    public void UploadEndpoints_AreRateLimited(Type controller, string action)
    {
        var method = controller.GetMethod(action);

        Assert.NotNull(method);

        var policy = method!.GetCustomAttributes<EnableRateLimitingAttribute>()
            .Select(attribute => attribute.PolicyName)
            .SingleOrDefault();

        Assert.Equal(RateLimitPolicies.Uploads, policy);
    }

    [Fact]
    public void UploadsPolicy_IsItsOwnAndNotWrites()
    {
        // Compartiendo el límite de escrituras, publicar diez posts y subir
        // cien fotos en el mismo minuto no frenaría nada: son rutas distintas
        // con costos muy distintos.
        Assert.NotEqual(RateLimitPolicies.Writes, RateLimitPolicies.Uploads);
    }
}
