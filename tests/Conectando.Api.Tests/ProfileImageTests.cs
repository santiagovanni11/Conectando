using Conectando.Api.Data;
using Conectando.Api.DTOs.Users;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

/// <summary>
/// Foto de perfil: cuota y limpieza del archivo anterior.
/// </summary>
/// <remarks>
/// La limpieza es lo que máscostó antes: con solo la URL guardada no había
/// forma de borrar el avatar viejo, y cada cambio dejaba una imagen huérfana
/// en Cloudinary pagando indefinidamente.
/// </remarks>
[Collection("Social")]
public class ProfileImageTests(SocialTestFixture fixture)
{
    private static (UserProfileService Service, FakeMediaStorage Storage) Create(
        ConectandoDbContext db, long? maxQuotaBytes = null)
    {
        var storage = new FakeMediaStorage();
        return (new UserProfileService(db, storage, StorageServices.Cleaner(storage), StorageServices.Quota(db, maxQuotaBytes)), storage);
    }

    [Fact]
    public async Task Upload_StoresSizeAndPublicId()
    {
        await using var db = fixture.CreateContext();
        var (service, _) = Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        var profile = await service.UploadProfileImageAsync(users[0].Id, PostTestData.JpegFile());

        Assert.NotNull(profile.ProfileImageUrl);
        var user = await db.Users.AsNoTracking().SingleAsync(u => u.Id == users[0].Id);
        Assert.Equal(8, user.ProfileImageSizeBytes);
        Assert.False(string.IsNullOrEmpty(user.ProfileImagePublicId));
    }

    [Fact]
    public async Task Upload_OverQuota_RejectedWithoutUploading()
    {
        await using var db = fixture.CreateContext();
        var (service, storage) = Create(db, maxQuotaBytes: 100);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<StorageQuotaExceededException>(() =>
            service.UploadProfileImageAsync(users[0].Id, PostTestData.JpegFile(length: 200)));

        Assert.Empty(storage.Uploaded);
    }

    [Fact]
    public async Task ChangingAvatar_DestroysThePreviousOne()
    {
        await using var db = fixture.CreateContext();
        var (service, storage) = Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await service.UploadProfileImageAsync(users[0].Id, PostTestData.JpegFile("uno.jpg"));
        var first = (await db.Users.AsNoTracking().SingleAsync(u => u.Id == users[0].Id)).ProfileImagePublicId;

        await service.UploadProfileImageAsync(users[0].Id, PostTestData.JpegFile("dos.jpg"));

        // El primero se va: sin esto quedaba pagando para siempre.
        Assert.Equal(2, storage.Uploaded.Count);
        Assert.Contains(first!, storage.Destroyed);
    }

    [Fact]
    public async Task FirstAvatar_HasNothingToDestroy()
    {
        await using var db = fixture.CreateContext();
        var (service, storage) = Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await service.UploadProfileImageAsync(users[0].Id, PostTestData.JpegFile());

        Assert.Empty(storage.Destroyed);
    }

    [Fact]
    public async Task ClearingAvatar_StopsCountingIt()
    {
        await using var db = fixture.CreateContext();
        var (service, storage) = Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        await service.UploadProfileImageAsync(users[0].Id, PostTestData.JpegFile());
        var previous = (await db.Users.AsNoTracking().SingleAsync(u => u.Id == users[0].Id)).ProfileImagePublicId;

        await service.UpdateProfileAsync(users[0].Id, new UpdateProfileRequest
        {
            DisplayName = users[0].DisplayName,
            ProfileImageUrl = null,
        });

        var user = await db.Users.AsNoTracking().SingleAsync(u => u.Id == users[0].Id);
        Assert.Null(user.ProfileImagePublicId);
        Assert.Equal(0, user.ProfileImageSizeBytes);
        Assert.Contains(previous!, storage.Destroyed);
    }

    [Fact]
    public async Task SavingProfile_KeepsAvatarWhenUrlDoesNotChange()
    {
        // El formulario de edición manda la URL actual de vuelta. Si eso
        // contara como cambio, cada guardado borraría el avatar sin motivo.
        await using var db = fixture.CreateContext();
        var (service, storage) = Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var uploaded = await service.UploadProfileImageAsync(users[0].Id, PostTestData.JpegFile());

        await service.UpdateProfileAsync(users[0].Id, new UpdateProfileRequest
        {
            DisplayName = "Nuevo nombre",
            ProfileImageUrl = uploaded.ProfileImageUrl,
        });

        var user = await db.Users.AsNoTracking().SingleAsync(u => u.Id == users[0].Id);
        Assert.Equal("Nuevo nombre", user.DisplayName);
        Assert.Equal(8, user.ProfileImageSizeBytes);
        Assert.Empty(storage.Destroyed);
    }
}
