using Conectando.Api.DTOs.Account;
using Conectando.Api.Interfaces;
using Conectando.Api.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Conectando.Api.Controllers;

/// <summary>
/// Recuperar el acceso a la cuenta.
///
/// Va aparte de <see cref="AuthController"/> a propósito: no autentica a
/// nadie, y el límite de intentos que tiene es de otra clase. Si mañana se
/// desactiva la cuenta o se cambia una regla de seguridad, el arranque y el
/// login no se tocan.
/// </summary>
[ApiController]
[Route("api/auth/password-reset")]
public class PasswordResetController(IPasswordResetService passwordReset) : ControllerBase
{
    private readonly IPasswordResetService _passwordReset = passwordReset;

    /// <summary>
    /// Pide un código.
    ///
    /// Responde 202 siempre, exista o no la cuenta. Si devolviera un 404 para
    /// los correos que no están, cualquiera podría recorrer la base probando
    /// direcciones y armarse una lista de quién usa la app.
    /// </summary>
    [HttpPost("request")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.PasswordReset)]
    public async Task<IActionResult> Request(
        RequestPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        await _passwordReset.RequestCodeAsync(request.Email, cancellationToken);
        return Accepted();
    }

    /// <summary>Comprueba el código y deja la contraseña nueva puesta.</summary>
    [HttpPost("confirm")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.PasswordReset)]
    public async Task<IActionResult> Confirm(
        ConfirmPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        await _passwordReset.ConfirmAsync(request, cancellationToken);
        return NoContent();
    }
}
