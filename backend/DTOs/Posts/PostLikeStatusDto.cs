namespace Conectando.Api.DTOs.Posts;

public class PostLikeStatusDto
{
    public int LikesCount { get; init; }
    public bool LikedByMe { get; init; }
}

public class PostSaveStatusDto
{
    public bool SavedByMe { get; init; }
}

public class LikeUserDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? ProfileImageUrl { get; init; }
}

public class LikePageDto
{
    public IReadOnlyList<LikeUserDto> Items { get; init; } = Array.Empty<LikeUserDto>();
    public bool HasMore { get; init; }
    public string? NextCursor { get; init; }
}