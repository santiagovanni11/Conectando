namespace Conectando.Api.Models;

/// <summary>
/// Me gusta de un comentario.
///
/// <para>
/// Va en su propia tabla y no como una lista dentro del comentario porque el
/// conteo se consulta siempre: el feed trae veinte comentarios y con una
/// colección cargada por cada uno el costo crece con el tamaño de la página.
/// Acá se cuenta con un <c>GROUP BY</c> sobre una sola consulta.
/// </para>
///
/// <para>
/// La clave es compuesta —comentario y usuario— y no un id propio: así la base
/// impide que alguien PONGA el me gusta dos veces sin que haya que comprobarlo
/// antes.
/// </para>
/// </summary>
public class CommentLike
{
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Comment Comment { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}