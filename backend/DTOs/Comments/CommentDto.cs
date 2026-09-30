namespace Conectando.Api.DTOs.Comments;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public CommentAuthorDto Author { get; set; } = null!;
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }
    public int RepliesCount { get; set; }

    /// <summary>Cuántos me gusta lleva el comentario.</summary>
    public int LikesCount { get; set; }

    /// <summary>Lo puso quien está mirando. Solo esa persona lo ve.</summary>
    public bool LikedByMe { get; set; }
}