using Conectando.Api.Data;
using Conectando.Api.DTOs.Social;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public class FollowService(ConectandoDbContext dbContext, IBlockService blockService, INotificationService notifications) : IFollowService
{
    private readonly ConectandoDbContext _dbContext = dbContext;
    private readonly IBlockService _blockService = blockService;
    private readonly INotificationService _notifications = notifications;

    public async Task FollowAsync(Guid userId, Guid targetUserId, CancellationToken cancellationToken = default)
    {
        if (userId == targetUserId)
        {
            throw new SelfActionException();
        }

        if (!await _dbContext.Users.AnyAsync(u => u.Id == targetUserId, cancellationToken))
        {
            throw new UserNotFoundException();
        }

        if (await _blockService.IsBlockedAsync(userId, targetUserId, cancellationToken))
        {
            throw new BlockedActionException();
        }

        if (await _dbContext.Follows.AnyAsync(
                f => f.UserId == userId && f.TargetUserId == targetUserId, cancellationToken))
        {
            return;
        }

        _dbContext.Follows.Add(new Follow { UserId = userId, TargetUserId = targetUserId });

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Solo la primera vez: si ya seguía, volver a tocar el botón
            // no debe generar un aviso nuevo.
            await _notifications.NotifyAsync(
                targetUserId, userId, NotificationTypeRequest.Follow, null, cancellationToken);
        }
        catch (DbUpdateException ex) when (PostgresErrors.IsUniqueViolation(ex))
        {
            // Dos navegaciones simultáneas: ya estaba, no se notifica.
        }
    }

    public async Task UnfollowAsync(Guid userId, Guid targetUserId, CancellationToken cancellationToken = default)
    {
        if (userId == targetUserId)
        {
            throw new SelfActionException();
        }

        var follow = await _dbContext.Follows
            .FirstOrDefaultAsync(f => f.UserId == userId && f.TargetUserId == targetUserId, cancellationToken);

        if (follow is null)
        {
            return;
        }

        _dbContext.Follows.Remove(follow);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<FollowDto>> GetFollowersAsync(Guid targetUserId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Follows
            .AsNoTracking()
            .Where(f => f.TargetUserId == targetUserId)
            .Join(_dbContext.Users.AsNoTracking(), f => f.UserId, u => u.Id, (f, u) => new FollowDto
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
    }

    public async Task<List<FollowDto>> GetFollowingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Follows
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Join(_dbContext.Users.AsNoTracking(), f => f.TargetUserId, u => u.Id, (f, u) => new FollowDto
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
    }
}