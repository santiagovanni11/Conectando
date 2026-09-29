using Conectando.Api.Data;
using Conectando.Api.DTOs.Social;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Solicitudes de amistad: enviar, aceptar, rechazar y cancelar.
/// Las consultas viven en `FriendRequestService.Queries.cs`.
/// </summary>
public partial class FriendRequestService(ConectandoDbContext dbContext, IBlockService blockService, INotificationService notifications) : IFriendRequestService
{
    private readonly ConectandoDbContext _dbContext = dbContext;
    private readonly IBlockService _blockService = blockService;
    private readonly INotificationService _notifications = notifications;

    public async Task SendAsync(Guid requesterId, Guid addresseeId, CancellationToken cancellationToken = default)
    {
        if (requesterId == addresseeId)
        {
            throw new SelfActionException();
        }
        if (!await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Id == addresseeId, cancellationToken))
        {
            throw new UserNotFoundException();
        }
        if (await _blockService.IsBlockedAsync(requesterId, addresseeId, cancellationToken))
        {
            throw new BlockedActionException();
        }
        if (await AreFriendsAsync(requesterId, addresseeId, cancellationToken))
        {
            throw new AlreadyFriendsException();
        }
        if (await _dbContext.FriendRequests.AnyAsync(
                r => (r.RequesterId == requesterId && r.AddresseeId == addresseeId)
                     || (r.RequesterId == addresseeId && r.AddresseeId == requesterId),
                cancellationToken))
        {
            throw new FriendRequestExistsException();
        }
        _dbContext.FriendRequests.Add(new FriendRequest { RequesterId = requesterId, AddresseeId = addresseeId });
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _notifications.NotifyAsync(addresseeId, requesterId, NotificationTypeRequest.FriendRequest, null, cancellationToken);
        }
        catch (DbUpdateException ex) when (PostgresErrors.IsUniqueViolation(ex))
        {
            throw new FriendRequestExistsException();
        }
    }

    public async Task AcceptAsync(Guid actorId, Guid requesterId, CancellationToken cancellationToken = default)
    {
        if (actorId == requesterId)
        {
            throw new NotRequestOwnerException();
        }
        var request = await _dbContext.FriendRequests
            .FirstOrDefaultAsync(r => r.RequesterId == requesterId && r.AddresseeId == actorId, cancellationToken);
        if (request is null)
        {
            if (await AreFriendsAsync(actorId, requesterId, cancellationToken))
            {
                return;
            }
            throw new FriendRequestNotFoundException();
        }
        _dbContext.FriendRequests.Remove(request);
        var (low, high) = SocialGuidPair.Normalize(requesterId, actorId);
        _dbContext.Friendships.Add(new Friendship { UserLowId = low, UserHighId = high });
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _notifications.NotifyAsync(requesterId, actorId, NotificationTypeRequest.FriendAccepted, null, cancellationToken);
        }
        catch (DbUpdateException ex) when (PostgresErrors.IsUniqueViolation(ex))
        {
            throw new AlreadyFriendsException();
        }
    }

    public async Task RejectAsync(Guid actorId, Guid requesterId, CancellationToken cancellationToken = default)
    {
        var request = await _dbContext.FriendRequests
            .FirstOrDefaultAsync(r => r.RequesterId == requesterId && r.AddresseeId == actorId, cancellationToken);
        if (request is null)
        {
            return;
        }
        _dbContext.FriendRequests.Remove(request);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(Guid requesterId, Guid addresseeId, CancellationToken cancellationToken = default)
    {
        var request = await _dbContext.FriendRequests
            .FirstOrDefaultAsync(r => r.RequesterId == requesterId && r.AddresseeId == addresseeId, cancellationToken);
        if (request is null)
        {
            return;
        }
        _dbContext.FriendRequests.Remove(request);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}