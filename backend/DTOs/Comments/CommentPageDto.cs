namespace Conectando.Api.DTOs.Comments;

public class CommentPageDto
{
    public IReadOnlyList<CommentDto> Items { get; set; } = Array.Empty<CommentDto>();
    public bool HasMore { get; set; }
    public string? NextCursor { get; set; }
}