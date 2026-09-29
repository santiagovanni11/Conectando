namespace Conectando.Api.Models;

public class PostMedia
{
    public Guid Id { get; set; }
    public Guid? PostId { get; set; }
    public Guid UserId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public PostMediaState State { get; set; } = PostMediaState.Pending;
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Peso del archivo, para saber cuánto ocupa el usuario. Antes no se
    /// guardaba, y por eso no había forma de hacer la cuenta: los límites
    /// eran de cantidad de archivos, no de espacio. Con video esto pasa a
    /// ser indispensable.
    /// </summary>
    public long SizeBytes { get; set; }

    public Post? Post { get; set; }
    public AppUser User { get; set; } = null!;
}