using Conectando.Api.Data;
using Conectando.Api.DTOs.Social;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public class BlockService(ConectandoDbContext dbContext) : IBlockService
{
    private readonly ConectandoDbContext _dbContext = dbContext;

    public async Task BlockAsync(Guid userId, Guid targetUserId, CancellationToken cancellationToken = default)
    {
        if (userId == targetUserId)
        {
            throw new SelfActionException();
        }

        if (!await _dbContext.Users.AnyAsync(u => u.Id == targetUserId, cancellationToken))
        {
            throw new UserNotFoundException();
        }

        if (await _dbContext.Blocks.AnyAsync(
                b => b.UserId == userId && b.BlockedUserId == targetUserId, cancellationToken))
        {
            return;
        }

        var (low, high) = SocialGuidPair.Normalize(userId, targetUserId);

        _dbContext.Friendships.RemoveRange(
            _dbContext.Friendships.Where(f => f.UserLowId == low && f.UserHighId == high));

        _dbContext.FriendRequests.RemoveRange(_dbContext.FriendRequests.Where(r =>
            (r.RequesterId == userId && r.AddresseeId == targetUserId) ||
            (r.RequesterId == targetUserId && r.AddresseeId == userId)));

        _dbContext.Follows.RemoveRange(_dbContext.Follows.Where(f =>
            (f.UserId == userId && f.TargetUserId == targetUserId) ||
            (f.UserId == targetUserId && f.TargetUserId == userId)));

        _dbContext.Blocks.Add(new Block { UserId = userId, BlockedUserId = targetUserId });

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (PostgresErrors.IsUniqueViolation(ex))
        {
        }
    }

    public async Task UnblockAsync(Guid userId, Guid targetUserId, CancellationToken cancellationToken = default)
    {
        var block = await _dbContext.Blocks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.BlockedUserId == targetUserId, cancellationToken);

        if (block is null)
        {
            return;
        }

        _dbContext.Blocks.Remove(block);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<BlockDto>> GetBlocksAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Blocks
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .Join(_dbContext.Users.AsNoTracking(), b => b.BlockedUserId, u => u.Id, (b, u) => new BlockDto
            {
                User = new UserSummaryDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    DisplayName = u.DisplayName,
                    ProfileImageUrl = u.ProfileImageUrl,
                },
                BlockedAt = b.CreatedAt,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsBlockedAsync(Guid userId, Guid targetUserId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Blocks.AsNoTracking().AnyAsync(b =>
            (b.UserId == userId && b.BlockedUserId == targetUserId) ||
            (b.UserId == targetUserId && b.BlockedUserId == userId), cancellationToken);
    }
}