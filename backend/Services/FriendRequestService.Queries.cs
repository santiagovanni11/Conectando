using Conectando.Api.DTOs.Social;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Consultas de solicitudes de amistad: las recibidas y las enviadas.
/// Se separan de los comandos para que este archivo no crezca sin límite.
/// </summary>
public partial class FriendRequestService
{
    public async Task<List<FriendRequestDto>> GetReceivedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FriendRequests
            .AsNoTracking()
            .Where(r => r.AddresseeId == userId)
            .Join(_dbContext.Users.AsNoTracking(), r => r.RequesterId, u => u.Id, (r, u) => new FriendRequestDto
            {
                User = new UserSummaryDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    DisplayName = u.DisplayName,
                    ProfileImageUrl = u.ProfileImageUrl,
                },
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FriendRequestDto>> GetSentAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FriendRequests
            .AsNoTracking()
            .Where(r => r.RequesterId == userId)
            .Join(_dbContext.Users.AsNoTracking(), r => r.AddresseeId, u => u.Id, (r, u) => new FriendRequestDto
            {
                User = new UserSummaryDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    DisplayName = u.DisplayName,
                    ProfileImageUrl = u.ProfileImageUrl,
                },
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<bool> AreFriendsAsync(Guid a, Guid b, CancellationToken cancellationToken)
    {
        var (low, high) = SocialGuidPair.Normalize(a, b);
        return await _dbContext.Friendships
            .AnyAsync(f => f.UserLowId == low && f.UserHighId == high, cancellationToken);
    }
}