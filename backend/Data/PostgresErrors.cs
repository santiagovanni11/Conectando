using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Conectando.Api.Data;

public static class PostgresErrors
{
    public const string UniqueViolation = "23505";

    public static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException { SqlState: UniqueViolation };
    }
}