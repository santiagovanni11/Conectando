using Conectando.Api.Data;
using Conectando.Api.Services;
using Conectando.Api.Settings;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Conectando.Api.Tests.TestInfrastructure;

public static class StorageServices
{
    /// <summary>
    /// Tope por defecto: el mismo que corre en producción, para que los tests
    /// midan contra la cifra real y no contra un número inventado acá. Los
    /// tests que necesitan toparlo pasan uno chico explícito.
    /// </summary>
    public static StorageQuotaService Quota(ConectandoDbContext db, long? maxBytes = null) =>
        new(db, Options.Create(new StorageQuotaSettings
        {
            MaxBytesPerUser = maxBytes ?? new StorageQuotaSettings().MaxBytesPerUser,
        }));

    public static MediaCleaner Cleaner(FakeMediaStorage storage) =>
        new(storage, NullLogger<MediaCleaner>.Instance);
}
