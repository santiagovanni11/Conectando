using Conectando.Api.Settings;

namespace Conectando.Api.Exceptions;

public class ConversationNotFoundException : ApiException
{
    public ConversationNotFoundException() : base(StatusCodes.Status404NotFound, "La conversación no existe o no sos parte de ella.") { }
}

public class EmptyMessageException : ApiException
{
    public EmptyMessageException() : base(StatusCodes.Status400BadRequest, "El mensaje no puede estar vacío.") { }
}

public class MessageTooLongException : ApiException
{
    public MessageTooLongException() : base(StatusCodes.Status400BadRequest, $"El mensaje no puede superar los {MessageLimits.MaxLength} caracteres.") { }
}

public class MessageNotFoundException : ApiException
{
    public MessageNotFoundException() : base(StatusCodes.Status404NotFound, "El mensaje no existe o fue eliminado.") { }
}

public class MessageOwnershipException : ApiException
{
    public MessageOwnershipException() : base(StatusCodes.Status403Forbidden, "Solo podés editar o eliminar tus propios mensajes.") { }
}

public class MessageEditExpiredException : ApiException
{
    public MessageEditExpiredException(int minutes)
        : base(StatusCodes.Status400BadRequest, $"Solo se puede editar un mensaje durante los primeros {minutes} minutos.")
    {
    }
}