namespace Conectando.Api.DTOs.Posts;

public class PostMediaDto
{
    public Guid Id { get; init; }
    public string Url { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}