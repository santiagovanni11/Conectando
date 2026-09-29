namespace Conectando.Api.Services;

/// <summary>
/// Las reglas de una contraseña aceptable, en un solo lugar.
/// </summary>
/// <remarks>
/// El front ya exigía ocho caracteres con letra y número, pero el servidor
/// solo miraba el largo. Eso dejaba dos puertas: por API se podía registrar
/// una contraseña que la interfaz nunca habría aceptado, y esa cuenta
/// después no podría cambiar su propia contraseña. Ahora manda acá.
/// </remarks>
public static class PasswordPolicy
{
    public const int MinLength = 8;

    /// <summary>Devuelve el motivo por el que no sirve, o null si sirve.</summary>
    public static string? Validate(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return "La contraseña es obligatoria.";
        }

        if (password.Length < MinLength)
        {
            return $"La contraseña debe tener al menos {MinLength} caracteres.";
        }

        if (!password.Any(char.IsLetter))
        {
            return "La contraseña debe incluir al menos una letra.";
        }

        if (!password.Any(char.IsDigit))
        {
            return "La contraseña debe incluir al menos un número.";
        }

        return null;
    }
}
