using Conectando.Api.Data;
using Conectando.Api.Services;

namespace Conectando.Api.Tests.TestInfrastructure;

public static class SocialServices
{
    public static (
        FriendRequestService Requests,
        FriendshipService Friends,
        FollowService Follows,
        BlockService Blocks,
        SocialStatusService Status) Create(ConectandoDbContext db)
    {
        var blocks = new BlockService(db);
        var notifications = new NullNotificationService();
        return (
            new FriendRequestService(db, blocks, notifications),
            new FriendshipService(db),
            new FollowService(db, blocks, notifications),
            blocks,
            new SocialStatusService(db));
    }
}