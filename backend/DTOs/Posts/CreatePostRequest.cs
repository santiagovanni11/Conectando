using Conectando.Api.Models;

namespace Conectando.Api.DTOs.Posts;

public class CreatePostRequest
{
    public string? Content { get; init; }
    public PostPrivacy Privacy { get; init; } = PostPrivacy.Public;
    public List<Guid>? MediaIds { get; init; }
}