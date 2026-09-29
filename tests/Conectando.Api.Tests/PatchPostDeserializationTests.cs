using System.Text.Json;
using Conectando.Api.DTOs.Posts;
using Conectando.Api.Models;
using Conectando.Api.Tests.TestInfrastructure;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class PatchPostDeserializationTests(SocialTestFixture fixture)
{
    [Fact]
    public void Deserialize_PresentContent_SetsHasValueWithValue()
    {
        var request = JsonSerializer.Deserialize<PatchPostRequest>("{\"content\":\"nuevo\"}")!;

        Assert.True(request.Content.HasValue);
        Assert.Equal("nuevo", request.Content.Value);
        Assert.False(request.Privacy.HasValue);
    }

    [Fact]
    public void Deserialize_ExplicitNullContent_SetsHasValueWithNullValue()
    {
        var request = JsonSerializer.Deserialize<PatchPostRequest>("{\"content\":null}")!;

        Assert.True(request.Content.HasValue);
        Assert.Null(request.Content.Value);
    }

    [Fact]
    public void Deserialize_AbsentContent_LeavesHasValueFalse()
    {
        var request = JsonSerializer.Deserialize<PatchPostRequest>("{}")!;

        Assert.False(request.Content.HasValue);
        Assert.False(request.Privacy.HasValue);
    }

    [Fact]
    public void Deserialize_PresentPrivacy_SetsEnumValue()
    {
        var request = JsonSerializer.Deserialize<PatchPostRequest>("{\"privacy\":\"Private\"}")!;

        Assert.True(request.Privacy.HasValue);
        Assert.Equal(PostPrivacy.Private, request.Privacy.Value);
    }

    [Fact]
    public async Task Deserialize_ThenPatch_AppliesOnlyProvidedFields_EndToEnd()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var patch = PostServices.CreatePatch(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var post = await PostTestData.SeedPostAsync(db, users[0], "original", PostPrivacy.Public);

        var request = JsonSerializer.Deserialize<PatchPostRequest>("{\"privacy\":\"Private\"}")!;
        var updated = await patch.PatchAsync(post.Id, users[0].Id, request);

        Assert.Equal("original", updated.Content);
        Assert.Equal(PostPrivacy.Private, updated.Privacy);
        var stored = await read.GetByIdAsync(post.Id, users[0].Id);
        Assert.Equal("original", stored.Content);
        Assert.Equal(PostPrivacy.Private, stored.Privacy);
    }

    [Fact]
    public async Task Deserialize_NullContent_ThenPatch_WithMedia_ClearsContent_EndToEnd()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var patch = PostServices.CreatePatch(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var media = await PostTestData.SeedPendingAsync(db, users[0], 1);
        var post = await PostTestData.SeedPostAsync(db, users[0], "texto", PostPrivacy.Public, media: media);

        var request = JsonSerializer.Deserialize<PatchPostRequest>("{\"content\":null}")!;
        var updated = await patch.PatchAsync(post.Id, users[0].Id, request);

        Assert.Null(updated.Content);
        var stored = await read.GetByIdAsync(post.Id, users[0].Id);
        Assert.Null(stored.Content);
    }
}