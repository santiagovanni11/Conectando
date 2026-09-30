using Conectando.Api.DTOs.Social;

namespace Conectando.Api.Services;

/// <summary>
/// Cómo se ve una cuenta que ya no existe.
/// </summary>
/// <remarks>
/// Al darse de baja, la cuenta no se borra: los mensajes la apuntan y el chat
/// del otro tiene que quedar. La fila queda con el nombre de usuario
/// "eliminado-{id}" porque la base lo usa como clave y dos cuentas
/// eliminadas no pueden compartirlo.
///
/// <para>
/// Ese texto es un detalle de la base de datos y no debería verse nunca. En
/// pantalla, una cadena de treinta y dos hexadecimales no parece una cuenta
/// dada de baja: parece un error, y de paso confirma que ahí hubo alguien.
/// Todas las respuestas van a pasar por acá, así que el identificador no
/// depende de que cada consulta se acuerde de taparlo.
/// </para>
/// </remarks>
public static class UserPresentation
{
    /// <summary>Lo que se muestra en lugar del nombre de una cuenta eliminada.</summary>
    public const string DeletedDisplayName = "Cuenta eliminada";

    /// <summary>
    /// Decide qué se envía al cliente.
    ///
    /// <para>
    /// Con la cuenta viva se manda lo que hay. Con la eliminada se manda la
    /// etiqueta y nada de identificador: el nombre de usuario queda vacío
    /// para que ningún componente lo imprima por inercia, y la foto fuera
    /// para que no se vea el avatar de alguien que pidió que no existiera.
    /// </para>
    /// </summary>
    public static UserSummaryDto De(Guid id, string userName, string displayName, string? avatar, bool eliminada) =>
        eliminada
            ? new UserSummaryDto { Id = id, IsDeleted = true, DisplayName = DeletedDisplayName }
            : new UserSummaryDto
            {
                Id = id,
                UserName = userName,
                DisplayName = displayName,
                ProfileImageUrl = avatar,
            };
}