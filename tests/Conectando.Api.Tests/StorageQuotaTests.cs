using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

/// <summary>
/// El tope de espacio por usuario.
/// </summary>
/// <remarks>
/// Lo que importa acá no es solo que rechace, sino que rechace <em>antes</em>
/// de subir: si aceptara el archivo y lo rechazara después, el usuario
/// pagaría en Cloudinary por algo que no se usó. Por eso casi todos los
/// tests terminan mirando <c>storage.Uploaded</c>.
/// </remarks>
[Collection("Social")]
public class StorageQuotaTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task Usage_AddsUpPostMediaAndAvatar()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 1);
        var quota = StorageServices.Quota(db);

        await PostTestData.SeedPendingAsync(db, users[0], 2, sizeBytes: 1000);
        users[0].ProfileImageSizeBytes = 250;
        await db.SaveChangesAsync();

        Assert.Equal(2250, await quota.GetUsageAsync(users[0].Id));
    }

    [Fact]
    public async Task Usage_DoesNotCountOtherUsers()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var quota = StorageServices.Quota(db);

        await PostTestData.SeedPendingAsync(db, users[0], 1, sizeBytes: 5000);

        Assert.Equal(0, await quota.GetUsageAsync(users[1].Id));
    }

    [Fact]
    public async Task Upload_OverQuota_RejectedWithoutPayingStorage()
    {
        await using var db = fixture.CreateContext();
        var (_, _, media, storage) = PostServices.Create(db, maxQuotaBytes: 1000);
        var users = await TestUsers.SeedAsync(db, 1);
        await PostTestData.SeedPendingAsync(db, users[0], 1, sizeBytes: 900);

        await Assert.ThrowsAsync<StorageQuotaExceededException>(() =>
            media.UploadAsync(users[0].Id, [PostTestData.JpegFile(length: 500)]));

        // Lo caro: no se subió nada. Si se hubiera subido y después rechazado,
        // el usuario ya pagó ese archivo.
        Assert.Empty(storage.Uploaded);
    }

    [Fact]
    public async Task Upload_FitsExactlyInQuota_Accepted()
    {
        await using var db = fixture.CreateContext();
        var (_, _, media, storage) = PostServices.Create(db, maxQuotaBytes: 1000);
        var users = await TestUsers.SeedAsync(db, 1);
        await PostTestData.SeedPendingAsync(db, users[0], 1, sizeBytes: 500);

        var drafts = await media.UploadAsync(users[0].Id, [PostTestData.JpegFile(length: 500)]);

        Assert.Single(drafts);
        Assert.Single(storage.Uploaded);
    }

    [Fact]
    public async Task Upload_WholeBatchCheckedAtOnce()
    {
        // Archivo por archivo, el primero entraría y el segundo se rechazaría:
        // el usuario pagaría una subida que le quedó sin usar.
        await using var db = fixture.CreateContext();
        var (_, _, media, storage) = PostServices.Create(db, maxQuotaBytes: 1000);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<StorageQuotaExceededException>(() =>
            media.UploadAsync(users[0].Id,
                [PostTestData.JpegFile("1.jpg", 600), PostTestData.JpegFile("2.jpg", 600)]));

        Assert.Empty(storage.Uploaded);
    }

    [Fact]
    public async Task DeletingMedia_FreesSpace()
    {
        await using var db = fixture.CreateContext();
        var (_, _, media, _) = PostServices.Create(db, maxQuotaBytes: 1000);
        var users = await TestUsers.SeedAsync(db, 1);
        var drafts = await media.UploadAsync(users[0].Id, [PostTestData.JpegFile(length: 1000)]);

        // Justo en el límite: no entra ni un byte más.
        await Assert.ThrowsAsync<StorageQuotaExceededException>(() =>
            media.UploadAsync(users[0].Id, [PostTestData.JpegFile(length: 1)]));

        await media.DeleteAsync(drafts[0].Id, users[0].Id);

        var freed = await media.UploadAsync(users[0].Id, [PostTestData.JpegFile(length: 1000)]);
        Assert.Single(freed);
    }

    [Fact]
    public async Task Upload_StoresSizeSoLaterChecksSeeIt()
    {
        // Si el peso no quedara guardado en la fila, la cuenta volvería a
        // cero en la siguiente subida y el tope no existiría.
        await using var db = fixture.CreateContext();
        var (_, _, media, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await media.UploadAsync(users[0].Id, [PostTestData.JpegFile(length: 4321)]);

        var saved = await db.PostMedia.AsNoTracking()
            .SingleAsync(m => m.UserId == users[0].Id);

        Assert.Equal(4321, saved.SizeBytes);
    }
}
