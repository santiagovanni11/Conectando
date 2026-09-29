namespace Conectando.Api.Exceptions;

public class InvalidCursorException : ApiException
{
    public InvalidCursorException() : base(StatusCodes.Status400BadRequest, "El cursor proporcionado no es válido.") { }
}
