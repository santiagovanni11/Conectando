namespace Conectando.Api.Exceptions;

public class CommentNotFoundException : ApiException
{
    public CommentNotFoundException() : base(StatusCodes.Status404NotFound, "El comentario no existe o no está disponible.") { }
}

public class CommentOwnershipException : ApiException
{
    public CommentOwnershipException() : base(StatusCodes.Status403Forbidden, "No podés realizar esta acción sobre un comentario ajeno.") { }
}

public class EmptyCommentException : ApiException
{
    public EmptyCommentException() : base(StatusCodes.Status400BadRequest, "El comentario no puede estar vacío.") { }
}

public class CommentTooLongException : ApiException
{
    public CommentTooLongException() : base(StatusCodes.Status400BadRequest, "El comentario es demasiado largo.") { }
}

public class InvalidParentCommentException : ApiException
{
    public InvalidParentCommentException(string message) : base(StatusCodes.Status400BadRequest, message) { }
}