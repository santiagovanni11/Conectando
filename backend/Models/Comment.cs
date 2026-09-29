namespace Conectando.Api.Models;

public class Comment
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Post Post { get; set; } = null!;
    public AppUser Author { get; set; } = null!;
    public Comment? ParentComment { get; set; }
}