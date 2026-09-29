using Conectando.Api.DTOs.Social;
using Conectando.Api.Models;

namespace Conectando.Api.DTOs.Posts;

public class PostDto
{
    public Guid Id { get; init; }
    public UserSummaryDto Author { get; init; } = null!;
    public string? Content { get; init; }
    public PostPrivacy Privacy { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public List<PostMediaDto> Media { get; init; } = [];
    public int LikesCount { get; init; }
    public bool LikedByMe { get; init; }
    public int CommentsCount { get; init; }

    /// <summary>El usuario actual la guardó. Solo él lo ve.</summary>
    public bool SavedByMe { get; init; }
}