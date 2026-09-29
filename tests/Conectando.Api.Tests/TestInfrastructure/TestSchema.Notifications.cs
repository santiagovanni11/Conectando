namespace Conectando.Api.Tests.TestInfrastructure;

public static partial class TestSchema
{
    /// <summary>
    /// Notificaciones. El enum se guarda como texto, igual que en la app real.
    /// </summary>
    private const string NotificationsTablesSql = """
        CREATE TABLE "notifications" (
            "Id" uuid NOT NULL,
            "RecipientId" uuid NOT NULL,
            "ActorId" uuid NOT NULL,
            "Type" character varying(50) NOT NULL,
            "PostId" uuid NULL,
            "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
            "ReadAt" timestamp with time zone NULL,
            CONSTRAINT "PK_notifications" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_notifications_users_ActorId" FOREIGN KEY ("ActorId") REFERENCES "users" ("Id"),
            CONSTRAINT "FK_notifications_users_RecipientId" FOREIGN KEY ("RecipientId") REFERENCES "users" ("Id"),
            CONSTRAINT "FK_notifications_posts_PostId" FOREIGN KEY ("PostId") REFERENCES "posts" ("Id")
        );
        CREATE INDEX "IX_notifications_ActorId" ON "notifications" ("ActorId");
        CREATE INDEX "IX_notifications_PostId" ON "notifications" ("PostId");
        CREATE INDEX "IX_notifications_RecipientId_CreatedAt_Id" ON "notifications" ("RecipientId", "CreatedAt", "Id");
        """;
}