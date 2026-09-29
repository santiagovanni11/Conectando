namespace Conectando.Api.DTOs.Users;

/// <summary>
/// Conteos de un perfil desde el punto de vista de quien lo mira.
/// Van aparte del perfil porque dependen del usuario que consulta.
/// </summary>
public class ProfileCountsDto
{
    public int PostsCount { get; init; }
    public int FriendsCount { get; init; }
    public int FollowersCount { get; init; }
}

public class PublicProfileDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Bio { get; init; }
    public string? ProfileImageUrl { get; init; }
    public bool IsPrivate { get; init; }
    public DateTime CreatedAt { get; init; }
}