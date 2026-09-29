using Conectando.Api.Data;
using Conectando.Api.DTOs.Social;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public class FriendshipService(ConectandoDbContext dbContext) : IFriendshipService
{
    private readonly ConectandoDbContext _dbContext = dbContext;

    public async Task<List<FriendDto>> GetFriendsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var lowerSide = await _dbContext.Friendships
            .AsNoTracking()
            .Where(f => f.UserHighId == userId)
            .Join(_dbContext.Users.AsNoTracking(), f => f.UserLowId, u => u.Id, (f, u) => new FriendDto
            {
                User = new UserSummaryDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    DisplayName = u.DisplayName,
                    ProfileImageUrl = u.ProfileImageUrl,
                },
                Since = f.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        var higherSide = await _dbContext.Friendships
            .AsNoTracking()
            .Where(f => f.UserLowId == userId)
            .Join(_dbContext.Users.AsNoTracking(), f => f.UserHighId, u => u.Id, (f, u) => new FriendDto
            {
                User = new UserSummaryDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    DisplayName = u.DisplayName,
                    ProfileImageUrl = u.ProfileImageUrl,
                },
                Since = f.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return [.. lowerSide, .. higherSide];
    }

    public async Task RemoveFriendAsync(Guid userId, Guid friendUserId, CancellationToken cancellationToken = default)
    {
        if (userId == friendUserId)
        {
            throw new SelfActionException();
        }

        var (low, high) = SocialGuidPair.Normalize(userId, friendUserId);
        var friendship = await _dbContext.Friendships
            .FirstOrDefaultAsync(f => f.UserLowId == low && f.UserHighId == high, cancellationToken);

        if (friendship is null)
        {
            return;
        }

        _dbContext.Friendships.Remove(friendship);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}