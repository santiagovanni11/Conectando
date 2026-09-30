namespace Conectando.Api.DTOs.Messages;

public class MessageDto
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public MessageAuthorDto Sender { get; init; } = null!;

    /// <summary>El otro ya lo leyó. Es el "visto" de Instagram.</summary>
    public bool IsSeenByPeer { get; init; }

    /// <summary>Se editó después de enviarlo; la interfaz muestra "Editado".</summary>
    public bool IsEdited { get; init; }

    /// <summary>
    /// Está borrado. Cuando es true, <see cref="Content"/> viene vacío para
    /// todos, incluido quien lo mandó: el texto deja de existir en el mundo.
    /// </summary>
    public bool IsDeleted { get; init; }

    /// <summary>
    /// Mensaje al que responde este, o null si no responde a ninguno.
    /// Viaja resuelto porque la burbuja lo muestra citado: si el front tuviera
    /// que pedirlo aparte, al abrir el hilo harían una consulta por mensaje.
    /// </summary>
    public MessageReplyDto? ReplyTo { get; init; }
}

/// <summary>
/// Cita del mensaje original dentro de una respuesta.
/// </summary>
public class MessageReplyDto
{
    public Guid Id { get; init; }
    public string SenderDisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Recorte del texto original. Viene recortado del servidor a propósito:
    /// el mensaje puede tener 2000 caracteres y la cita muestra una línea.
    /// </summary>
    public string Preview { get; init; } = string.Empty;

    /// <summary>El original ya no existe; la cita muestra un aviso.</summary>
    public bool IsDeleted { get; init; }
}

public class MessageAuthorDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? ProfileImageUrl { get; init; }
}

public class MessagePageDto
{
    public List<MessageDto> Items { get; init; } = [];
    public string? NextCursor { get; init; }
    public bool HasMore { get; init; }
}

public class SendMessageRequest
{
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Id del mensaje al que se responde, o null para enviar sin cita.
    /// Lo valida el servidor: tiene que ser de esta misma conversación.
    /// </summary>
    public Guid? ReplyToMessageId { get; set; }
}

public class EditMessageRequest
{
    public string Content { get; set; } = string.Empty;
}

public class MuteConversationRequest
{
    public bool Muted { get; set; }
}

public class MuteConversationResponse
{
    public bool IsMuted { get; init; }
}

public class StartConversationRequest
{
    public Guid RecipientId { get; set; }
}