namespace Conectando.Api.Exceptions;

public abstract class ApiException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class DuplicateUserNameException : ApiException
{
    public DuplicateUserNameException() : base(StatusCodes.Status409Conflict, "El nombre de usuario ya está registrado.") { }
}

public class DuplicateEmailException : ApiException
{
    public DuplicateEmailException() : base(StatusCodes.Status409Conflict, "El email ya está registrado.") { }
}

public class InvalidCredentialsException : ApiException
{
    public InvalidCredentialsException() : base(StatusCodes.Status401Unauthorized, "Email o contraseña incorrectos.") { }
}

public class UserNotFoundException : ApiException
{
    public UserNotFoundException() : base(StatusCodes.Status404NotFound, "El usuario no existe.") { }
}