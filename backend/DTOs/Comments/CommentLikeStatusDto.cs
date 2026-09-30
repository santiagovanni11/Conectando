namespace Conectando.Api.DTOs.Comments;

/// <summary>Cuántos me gusta lleva un comentario y si el que mira puso el suyo.</summary>
public class CommentLikeStatusDto
{
    public int LikesCount { get; set; }

    /// <summary>Lo puso la persona que está mirando. Solo ella lo ve.</summary>
    public bool LikedByMe { get; set; }
}