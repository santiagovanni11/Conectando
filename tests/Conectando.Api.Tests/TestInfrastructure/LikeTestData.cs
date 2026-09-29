using Conectando.Api.Data;
using Conectando.Api.Models;

namespace Conectando.Api.Tests.TestInfrastructure;

public static class LikeTestData
{
    public static async Task<Like> SeedLikeAsync(ConectandoDbContext db, Guid userId, Guid postId)
    {
        var like = new Like
        {
            UserId = userId,
            PostId = postId,
            CreatedAt = DateTime.UtcNow,
        };

        db.Likes.Add(like);
        await db.SaveChangesAsync();
        return like;
    }

    public static async Task SeedLikesAsync(ConectandoDbContext db, Guid postId, params Guid[] userIds)
    {
        var likes = userIds.Select(userId => new Like
        {
            UserId = userId,
            PostId = postId,
            CreatedAt = DateTime.UtcNow,
        }).ToList();

        db.Likes.AddRange(likes);
        await db.SaveChangesAsync();
    }
}