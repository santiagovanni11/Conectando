namespace Conectando.Api.Models;

/// <summary>
/// Publicación guardada por un usuario.
///
/// Es personal: nadie más ve lo que guardaste, ni siquiera el autor.
/// </summary>
public class PostSave
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public DateTime CreatedAt { get; set; }
}