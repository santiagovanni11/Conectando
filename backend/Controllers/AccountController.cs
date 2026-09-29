using Conectando.Api.DTOs.Account;
using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conectando.Api.Controllers;

/// <summary>
/// Gestión de la cuenta propia: contraseña y baja.
/// </summary>
/// <remarks>
/// Va aparte de <c>UsersController</c> a propósito: ese expone el perfil
/// como lo ven los demás, y esto solo puede hacerlo su dueño. Mezclarlos
/// dejaría un archivo que crece sin criterio, con endpoints de permisos
/// distintos al lado.
/// </remarks>
[ApiController]
[Route("api/account")]
[Authorize]
public class AccountController(IAccountService accounts) : ControllerBase
{
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        await accounts.ChangePasswordAsync(User.GetUserId(), request, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Da de baja la cuenta. Es DELETE y no POST porque la operación es
    /// borrar; el cuerpo llega igual, para poder exigir la contraseña.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteAccountRequest request,
        CancellationToken cancellationToken)
    {
        await accounts.DeleteAccountAsync(User.GetUserId(), request, cancellationToken);
        return NoContent();
    }
}
