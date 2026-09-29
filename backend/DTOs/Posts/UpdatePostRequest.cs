using Conectando.Api.Models;

namespace Conectando.Api.DTOs.Posts;

public class UpdatePostRequest
{
    public string? Content { get; init; }
    public PostPrivacy? Privacy { get; init; }
    public List<Guid>? MediaIds { get; init; }
}