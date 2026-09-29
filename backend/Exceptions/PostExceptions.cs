namespace Conectando.Api.Exceptions;

public class PostNotFoundException : ApiException
{
    public PostNotFoundException() : base(StatusCodes.Status404NotFound, "La publicación no existe o no está disponible.") { }
}

public class PostOwnershipException : ApiException
{
    public PostOwnershipException() : base(StatusCodes.Status403Forbidden, "No podés realizar esta acción sobre una publicación ajena.") { }
}

public class InvalidFileException : ApiException
{
    public InvalidFileException(string message) : base(StatusCodes.Status400BadRequest, message) { }
}

public class MediaLimitException : ApiException
{
    public MediaLimitException(string message) : base(StatusCodes.Status400BadRequest, message) { }
}

public class PendingMediaNotFoundException : ApiException
{
    public PendingMediaNotFoundException() : base(StatusCodes.Status404NotFound, "El archivo no existe o pertenece a otro usuario.") { }
}

public class EmptyPostException : ApiException
{
    public EmptyPostException() : base(StatusCodes.Status400BadRequest, "La publicación no puede estar vacía: escribí texto o agregá al menos una foto.") { }
}

public class MediaStorageException : ApiException
{
    public MediaStorageException() : base(StatusCodes.Status503ServiceUnavailable, "No se pudo almacenar el archivo. Intentá de nuevo en unos minutos.") { }
}

public class MediaStorageNotConfiguredException : ApiException
{
    public MediaStorageNotConfiguredException() : base(StatusCodes.Status503ServiceUnavailable, "El almacenamiento de imágenes no está configurado.") { }
}