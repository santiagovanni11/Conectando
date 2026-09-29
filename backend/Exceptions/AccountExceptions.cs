namespace Conectando.Api.Exceptions;

public class CurrentPasswordIncorrectException : ApiException
{
    public CurrentPasswordIncorrectException() : base(StatusCodes.Status401Unauthorized, "La contraseña actual no es correcta.") { }
}

public class WeakPasswordException(string reason) : ApiException(StatusCodes.Status400BadRequest, reason)
{
}

/// <summary>
/// Se lanza cuando la nueva contraseña es la misma que la actual: el usuario
/// está tratando de reforzar una que no cambió.
/// </summary>
public class SamePasswordException : ApiException
{
    public SamePasswordException() : base(StatusCodes.Status400BadRequest, "La nueva contraseña debe ser distinta de la actual.") { }
}

public class PasswordConfirmationMismatchException : ApiException
{
    public PasswordConfirmationMismatchException() : base(StatusCodes.Status400BadRequest, "Las contraseñas no coinciden.") { }
}
