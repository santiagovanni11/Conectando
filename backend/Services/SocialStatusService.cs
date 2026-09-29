using Conectando.Api.Data;
using Conectando.Api.DTOs.Social;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public class SocialStatusService(ConectandoDbContext dbContext) : ISocialStatusService
{
    private readonly ConectandoDbContext _dbContext = dbContext;

    public async Task<RelationshipStatusDto> GetStatusAsync(Guid userId, Guid targetUserId, CancellationToken cancellationToken = default)
    {
        if (userId == targetUserId)
        {
            throw new SelfActionException();
        }

        if (!await _dbContext.Users.AnyAsync(u => u.Id == targetUserId, cancellationToken))
        {
            throw new UserNotFoundException();
        }

        var (low, high) = SocialGuidPair.Normalize(userId, targetUserId);

        var currentUserId = userId;
        var targetId = targetUserId;

        var flags = await _dbContext.Users
            .Where(u => u.Id == targetId)
            .Select(u => new StatusFlags(
                _dbContext.Friendships.Any(f => f.UserLowId == low && f.UserHighId == high),
                _dbContext.FriendRequests.Any(r => r.RequesterId == currentUserId && r.AddresseeId == targetId),
                _dbContext.FriendRequests.Any(r => r.RequesterId == targetId && r.AddresseeId == currentUserId),
                _dbContext.Follows.Any(f => f.UserId == currentUserId && f.TargetUserId == targetId),
                _dbContext.Follows.Any(f => f.UserId == targetId && f.TargetUserId == currentUserId),
                _dbContext.Blocks.Any(b => b.UserId == currentUserId && b.BlockedUserId == targetId),
                _dbContext.Blocks.Any(b => b.UserId == targetId && b.BlockedUserId == currentUserId)))
            .SingleOrDefaultAsync(cancellationToken);

        if (flags is null)
        {
            throw new UserNotFoundException();
        }

        var friendship = flags.IsFriends ? "friends"
            : flags.RequestSent ? "sent"
            : flags.RequestReceived ? "received"
            : "none";

        return new RelationshipStatusDto
        {
            UserId = targetId,
            Friendship = friendship,
            Following = flags.Following,
            FollowedBy = flags.FollowedBy,
            BlockedByMe = flags.BlockedByMe,
            BlockedByThem = flags.BlockedByThem,
        };
    }

    private sealed record StatusFlags(
        bool IsFriends,
        bool RequestSent,
        bool RequestReceived,
        bool Following,
        bool FollowedBy,
        bool BlockedByMe,
        bool BlockedByThem);
}