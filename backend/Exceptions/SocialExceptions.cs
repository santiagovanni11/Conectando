namespace Conectando.Api.Exceptions;

public class SelfActionException : ApiException
{
    public SelfActionException() : base(StatusCodes.Status400BadRequest, "No podés realizar esta acción sobre tu propio usuario.") { }
}

public class AlreadyFriendsException : ApiException
{
    public AlreadyFriendsException() : base(StatusCodes.Status409Conflict, "Ya son amigos.") { }
}

public class FriendRequestExistsException : ApiException
{
    public FriendRequestExistsException() : base(StatusCodes.Status409Conflict, "Ya existe una solicitud de amistad entre ambos.") { }
}

public class BlockedActionException : ApiException
{
    public BlockedActionException() : base(StatusCodes.Status409Conflict, "No podés realizar esta acción porque hay un bloqueo de por medio.") { }
}

public class NotRequestOwnerException : ApiException
{
    public NotRequestOwnerException() : base(StatusCodes.Status403Forbidden, "No podés realizar esta acción sobre esa solicitud.") { }
}

public class FriendRequestNotFoundException : ApiException
{
    public FriendRequestNotFoundException() : base(StatusCodes.Status404NotFound, "No existe una solicitud de amistad pendiente.") { }
}