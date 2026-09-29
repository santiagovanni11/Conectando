namespace Conectando.Api.DTOs.Users;

/// <summary>
/// Usuario con su estado de relación respecto del que consulta.
/// Se usa en la búsqueda y en las sugerencias de amigos.
/// </summary>
public class UserCardDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Bio { get; init; }
    public string? ProfileImageUrl { get; init; }

    /// <summary>none | sent | received | friends</summary>
    public string Friendship { get; init; } = "none";

    public int FriendsCount { get; init; }
    public int MutualFriendsCount { get; init; }
}