namespace Conectando.Api.DTOs.Comments;

public class CommentAuthorDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
}