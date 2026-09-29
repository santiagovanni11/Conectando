namespace Conectando.Api.Models;

/// <summary>Una conversación entre dos o más personas.</summary>
public class Conversation
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Vincula un usuario con una conversación y marca hasta dónde leyó.
///
/// DeletedAt es un borrado por persona: cuando alguien elimina un chat
/// desaparece solo para esa persona, igual que en WhatsApp. Si la otra
/// persona sigue escribiendo, el chat vuelve a aparecer.
/// </summary>
public class ConversationMember
{
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LastReadAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Silenciada: el chat sigue existiendo y se puede seguir escribiendo,
    /// pero deja de generar la notificación. Es reversible.
    /// </summary>
    public DateTime? MutedAt { get; set; }

    public Conversation Conversation { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}

/// <summary>
/// Mensaje de una conversación.
///
/// Borrar un mensaje no lo elimina de la base: se marca IsDeleted y el
/// contenido deja de enviarse. Así el hilo no se rompe y, sobre todo, el
/// texto deja de existir para todos.
/// </summary>
public class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; }

    public Conversation Conversation { get; set; } = null!;
    public AppUser Sender { get; set; } = null!;
}