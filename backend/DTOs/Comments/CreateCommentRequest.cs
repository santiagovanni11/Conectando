namespace Conectando.Api.DTOs.Comments;

public class CreateCommentRequest
{
    public string Content { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
}