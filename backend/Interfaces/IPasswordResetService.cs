using Conectando.Api.DTOs.Account;

namespace Conectando.Api.Interfaces;

public interface IPasswordResetService
{
    /// <summary>
    /// Pide un código. No dice si la cuenta existe: esa respuesta se da
    /// siempre igual a propósito.
    /// </summary>
    Task RequestCodeAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Comprueba el código y cambia la contraseña.</summary>
    Task ConfirmAsync(ConfirmPasswordResetRequest request, CancellationToken cancellationToken = default);
}
