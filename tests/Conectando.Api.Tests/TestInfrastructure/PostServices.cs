using Conectando.Api.Data;
using Conectando.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Conectando.Api.Tests.TestInfrastructure;

public static class PostServices
{
    public static (
        PostReadService Read,
        PostWriteService Write,
        PostMediaService Media,
        FakeMediaStorage Storage) Create(ConectandoDbContext db) => Create(db, null);

    /// <summary>
    /// Igual que <see cref="Create(ConectandoDbContext)"/>, pero con un tope de
    /// espacio propio para poder probar que frena las subidas.
    /// </summary>
    public static (
        PostReadService Read,
        PostWriteService Write,
        PostMediaService Media,
        FakeMediaStorage Storage) Create(ConectandoDbContext db, long? maxQuotaBytes)
    {
        var storage = new FakeMediaStorage();
        var mediaUpdater = new PostMediaUpdater(db, storage, NullLogger<PostMediaUpdater>.Instance);
        return (
            new PostReadService(db, new PostVisibilityService(db)),
            new PostWriteService(db, mediaUpdater),
            new PostMediaService(db, storage, StorageServices.Cleaner(storage), StorageServices.Quota(db, maxQuotaBytes)),
            storage);
    }

    public static PostPatchService CreatePatch(ConectandoDbContext db)
    {
        var storage = new FakeMediaStorage();
        return new PostPatchService(db, new PostMediaUpdater(db, storage, NullLogger<PostMediaUpdater>.Instance));
    }
}