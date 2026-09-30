namespace Conectando.Api.DTOs.Comments;

public class CommentAuthorDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// La cuenta fue dada de baja. Cuando está en true no hay perfil al que ir
    /// y el nombre que se muestra es una etiqueta, no el identificador que
    /// quedó guardado en la base.
    /// </summary>
    public bool IsDeleted { get; set; }
}