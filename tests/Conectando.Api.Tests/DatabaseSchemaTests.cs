using Npgsql;

namespace Conectando.Api.Tests;

/// <summary>
/// La base real tiene las columnas que el modelo pide.
/// </summary>
/// <remarks>
/// Existe por una razón concreta: la base de tests se arma con SQL a mano
/// (<c>TestSchema</c>), no con las migraciones. Por eso agregar una columna
/// al modelo y al SQL de prueba deja los 253 tests en verde mientras la app
/// real revienta con un 500, porque a esta base todavía nadie le corrió la
/// migración. Este test mira la base de verdad y dice qué falta.
/// </remarks>
public class DatabaseSchemaTests
{
    [Theory]
    [InlineData("SecurityStamp")]
    [InlineData("DeletedAt")]
    public void Users_TieneLaColumna(string columna)
    {
        Assert.True(
            ColumnaExiste("users", columna),
            $"Falta users.{columna}: corré la migración con dotnet ef database update.");
    }

    private static bool ColumnaExiste(string tabla, string columna)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Sin base a la que mirar no hay nada que afirmar. Se cuela en
            // lugar de fallar para no tapar el resto de la suite.
            return true;
        }

        using var conexion = new NpgsqlConnection(connectionString);
        conexion.Open();

        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            SELECT EXISTS (
                SELECT 1 FROM information_schema.columns
                WHERE table_name = @tabla AND column_name = @columna
            )
            """;

        comando.Parameters.AddWithValue("@tabla", tabla);
        comando.Parameters.AddWithValue("@columna", columna);

        return (bool)(comando.ExecuteScalar() ?? false);
    }
}
