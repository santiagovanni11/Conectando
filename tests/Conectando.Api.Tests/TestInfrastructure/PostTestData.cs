using Conectando.Api.Data;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests.TestInfrastructure;

public static class PostTestData
{
    public static byte[] ValidJpegBytes { get; } = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };

    public static IFormFile JpegFile(string name = "foto.jpg", long length = 8)
    {
        var stream = new MemoryStream(ValidJpegBytes);
        return new FormFile(stream, 0, length, "files", name)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg",
        };
    }

    public static async Task<List<PostMedia>> SeedPendingAsync(ConectandoDbContext db, AppUser user, int count, long sizeBytes = 0)
    {
        var media = Enumerable.Range(0, count).Select(i => new PostMedia
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Url = $"https://media.test/{Guid.NewGuid():N}.jpg",
            PublicId = $"posts/{Guid.NewGuid():N}",
            DisplayOrder = i,
            State = PostMediaState.Pending,
            CreatedAt = DateTime.UtcNow,
            SizeBytes = sizeBytes,
        }).ToList();

        db.PostMedia.AddRange(media);
        await db.SaveChangesAsync();
        return media;
    }

    public static async Task<Post> SeedPostAsync(ConectandoDbContext db, AppUser author, string? content,
        PostPrivacy privacy, DateTime? createdAt = null, List<PostMedia>? media = null)
    {
        var now = createdAt ?? DateTime.UtcNow;
        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorId = author.Id,
            Content = content,
            Privacy = privacy,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Posts.Add(post);
        await db.SaveChangesAsync();

        if (media is not null)
        {
            for (var i = 0; i < media.Count; i++)
            {
                media[i].Post = post;
                media[i].State = PostMediaState.Attached;
                media[i].DisplayOrder = i;
            }

            await db.SaveChangesAsync();
        }

        return post;
    }

    public static async Task SeedFriendshipAsync(ConectandoDbContext db, Guid a, Guid b)
    {
        var (low, high) = SocialGuidPair.Normalize(a, b);
        db.Friendships.Add(new Friendship { UserLowId = low, UserHighId = high, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
    }

    public static async Task SeedFollowAsync(ConectandoDbContext db, Guid followerId, Guid targetId)
    {
        db.Follows.Add(new Follow { UserId = followerId, TargetUserId = targetId, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
    }
}