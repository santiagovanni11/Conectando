namespace Conectando.Api.DTOs.Posts;

public class PostListDto
{
    public List<PostDto> Items { get; init; } = [];
    public string? NextCursor { get; init; }
    public bool HasMore { get; init; }
}

public class PostListQuery
{
    public string? Cursor { get; init; }
    public int Limit { get; init; } = 10;
}