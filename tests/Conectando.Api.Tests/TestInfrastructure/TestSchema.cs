namespace Conectando.Api.Tests.TestInfrastructure;

/// <summary>
/// Schema de la base de pruebas.
///
/// Los CREATE TABLE están repartidos en archivos parciales por dominio
/// (Social, Posts, Messages, Notifications) y se ensamblan acá. El orden
/// importa: hay claves foráneas, así que las tablas tienen que crearse
/// después de las que referencian.
/// </summary>
public static partial class TestSchema
{
    public const string DropTablesSql = """
        DROP TABLE IF EXISTS "comments" CASCADE;
        DROP TABLE IF EXISTS "notifications" CASCADE;
        DROP TABLE IF EXISTS "post_media" CASCADE;
        DROP TABLE IF EXISTS "post_saves" CASCADE;
        DROP TABLE IF EXISTS "shares" CASCADE;
        DROP TABLE IF EXISTS "likes" CASCADE;
        DROP TABLE IF EXISTS "posts" CASCADE;
        DROP TABLE IF EXISTS "messages" CASCADE;
        DROP TABLE IF EXISTS "conversation_members" CASCADE;
        DROP TABLE IF EXISTS "conversations" CASCADE;
        DROP TABLE IF EXISTS "reports" CASCADE;
        DROP TABLE IF EXISTS "blocks" CASCADE;
        DROP TABLE IF EXISTS "follows" CASCADE;
        DROP TABLE IF EXISTS "friend_requests" CASCADE;
        DROP TABLE IF EXISTS "friendships" CASCADE;
        DROP TABLE IF EXISTS "password_reset_codes" CASCADE;
        DROP TABLE IF EXISTS "users" CASCADE;
        """;

    /// <summary>
    /// No puede ser <c>const</c>: concatena piezas definidas en otros
    /// archivos de la misma clase parcial.
    /// </summary>
    public static readonly string CreateTablesSql =
        SocialTablesSql
        + PostsTablesSql
        + MessagesTablesSql
        + NotificationsTablesSql;
}