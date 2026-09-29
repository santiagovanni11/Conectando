using Conectando.Api.Data;
using Conectando.Api.Models;

namespace Conectando.Api.Tests.TestInfrastructure;

public static class BlockTestData
{
    public static async Task SeedBlockAsync(ConectandoDbContext db, Guid blockerId, Guid blockedId)
    {
        db.Blocks.Add(new Block { UserId = blockerId, BlockedUserId = blockedId, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
    }
}