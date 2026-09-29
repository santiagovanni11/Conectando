using Conectando.Api.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Conectando.Api.Tests.TestInfrastructure;

public sealed class SocialTestFixture : IAsyncLifetime
{
    private const string TestDatabase = "conectando_test";

    public DbContextOptions<ConectandoDbContext> Options { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var main = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? throw new InvalidOperationException("La variable ConnectionStrings__DefaultConnection no está definida.");

        await EnsureTestDatabaseExistsAsync(main);

        var testConnection = new NpgsqlConnectionStringBuilder(main)
        {
            Database = TestDatabase,
            Timeout = 60,
            MaxPoolSize = 20,
        }.ConnectionString;
        Options = new DbContextOptionsBuilder<ConectandoDbContext>().UseNpgsql(testConnection).Options;

        await using var bootstrap = new ConectandoDbContext(Options);
        await ExecuteWithLoginRetryAsync(bootstrap, TestSchema.DropTablesSql);
        await ExecuteWithLoginRetryAsync(bootstrap, TestSchema.CreateTablesSql);
    }

    private static async Task EnsureTestDatabaseExistsAsync(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString) { Database = "neondb" };
        await using var conn = new NpgsqlConnection(builder.ConnectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = '{TestDatabase}'", conn);
        var exists = await cmd.ExecuteScalarAsync() is not null;

        if (!exists)
        {
            await using var createCmd = new NpgsqlCommand($"CREATE DATABASE {TestDatabase}", conn);
            await createCmd.ExecuteNonQueryAsync();
        }
    }

    public ConectandoDbContext CreateContext() => new(Options);

    public async Task DisposeAsync()
    {
        await using var context = new ConectandoDbContext(Options);
        await ExecuteWithLoginRetryAsync(context, TestSchema.DropTablesSql);
    }

    private static async Task ExecuteWithLoginRetryAsync(ConectandoDbContext db, string sql)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await db.Database.ExecuteSqlRawAsync(sql);
                return;
            }
            catch (PostgresException ex) when (ex.SqlState is "08P01" or "3D000" or "28000" && attempt < 6)
            {
                await Task.Delay(TimeSpan.FromSeconds(5 * attempt));
            }
        }
    }
}