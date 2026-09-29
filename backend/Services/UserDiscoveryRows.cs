using Conectando.Api.DTOs.Users;

namespace Conectando.Api.Services;

internal sealed record CandidateRow(
    Guid Id,
    string UserName,
    string DisplayName,
    string? Bio,
    string? ProfileImageUrl);

/// <summary>Flags de relación y conteos para un candidato.</summary>
internal sealed record CandidateRelations(
    Guid Id,
    bool IsFriend,
    bool RequestSent,
    bool RequestReceived,
    int FriendsCount,
    int MutualFriendsCount);

internal sealed record FriendshipRow(Guid UserLowId, Guid UserHighId);

internal sealed record FriendRequestRow(Guid RequesterId, Guid AddresseeId);

/// <summary>
/// Calcula el estado de relación entre el usuario actual y un candidato.
/// Vive aparte para que <c>UserDiscoveryService</c> no crezca.
/// </summary>
internal static class UserConnectionMapper
{
    public static UserCardDto Map(CandidateRow row, CandidateRelations relations) => new()
    {
        Id = row.Id,
        UserName = row.UserName,
        DisplayName = row.DisplayName,
        Bio = row.Bio,
        ProfileImageUrl = row.ProfileImageUrl,
        Friendship = RelationResolver.FriendshipLabel(relations),
        FriendsCount = relations.FriendsCount,
        MutualFriendsCount = relations.MutualFriendsCount,
    };
}

internal static class RelationResolver
{
    public static CandidateRelations Resolve(
        Guid candidateId,
        Guid me,
        IReadOnlyCollection<FriendshipRow> friendships,
        IReadOnlyCollection<FriendRequestRow> requests)
    {
        var candidateFriends = Sides(friendships, candidateId);
        var myFriends = Sides(friendships, me);

        // Soy amigo del candidato si=share exactamente la misma amistad.
        var isFriend = AreFriends(friendships, me, candidateId);

        // Amigos en común: los que están en ambos conjuntos.
        var mutual = myFriends.Count(id => candidateFriends.Contains(id));

        return new CandidateRelations(
            candidateId,
            isFriend,
            requests.Any(r => r.RequesterId == me && r.AddresseeId == candidateId),
            requests.Any(r => r.RequesterId == candidateId && r.AddresseeId == me),
            candidateFriends.Count,
            mutual);
    }

    /// <summary>Ids del otro lado de cada amistad que toca a <paramref name="userId"/>.</summary>
    private static HashSet<Guid> Sides(IReadOnlyCollection<FriendshipRow> friendships, Guid userId) =>
    [
        .. friendships
            .Where(f => f.UserLowId == userId || f.UserHighId == userId)
            .Select(f => f.UserLowId == userId ? f.UserHighId : f.UserLowId),
    ];

    public static bool AreFriends(IReadOnlyCollection<FriendshipRow> friendships, Guid a, Guid b) =>
        friendships.Any(f =>
            (f.UserLowId == a && f.UserHighId == b) || (f.UserLowId == b && f.UserHighId == a));

    public static string FriendshipLabel(CandidateRelations relations) =>
        relations.IsFriend ? "friends"
        : relations.RequestSent ? "sent"
        : relations.RequestReceived ? "received"
        : "none";
}