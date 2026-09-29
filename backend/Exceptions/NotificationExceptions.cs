namespace Conectando.Api.Exceptions;

public class NotificationNotFoundException : ApiException
{
    public NotificationNotFoundException() : base(StatusCodes.Status404NotFound, "La notificación no existe.") { }
}