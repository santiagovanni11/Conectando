namespace Conectando.Api.Tests.TestInfrastructure;

public static partial class TestSchema
{
    /// <summary>Conversaciones, sus miembros y los mensajes.</summary>
    private const string MessagesTablesSql = """
        CREATE TABLE "conversations" (
            "Id" uuid NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
            "UpdatedAt" timestamp with time zone NOT NULL DEFAULT now(),
            CONSTRAINT "PK_conversations" PRIMARY KEY ("Id")
        );
        CREATE INDEX "IX_conversations_UpdatedAt" ON "conversations" ("UpdatedAt");
        CREATE TABLE "conversation_members" (
            "ConversationId" uuid NOT NULL,
            "UserId" uuid NOT NULL,
            "JoinedAt" timestamp with time zone NOT NULL DEFAULT now(),
            "LastReadAt" timestamp with time zone NULL,
            "DeletedAt" timestamp with time zone NULL,
            "MutedAt" timestamp with time zone NULL,
            CONSTRAINT "PK_conversation_members" PRIMARY KEY ("ConversationId", "UserId"),
            CONSTRAINT "FK_conversation_members_conversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "conversations" ("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_conversation_members_users_UserId" FOREIGN KEY ("UserId") REFERENCES "users" ("Id") ON DELETE CASCADE
        );
        CREATE INDEX "IX_conversation_members_UserId" ON "conversation_members" ("UserId");
        CREATE TABLE "messages" (
            "Id" uuid NOT NULL,
            "ConversationId" uuid NOT NULL,
            "SenderId" uuid NOT NULL,
            "Content" character varying(2000) NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
            "EditedAt" timestamp with time zone NULL,
            "IsDeleted" boolean NOT NULL DEFAULT false,
            "ReplyToMessageId" uuid NULL,
            CONSTRAINT "PK_messages" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_messages_conversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "conversations" ("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_messages_users_SenderId" FOREIGN KEY ("SenderId") REFERENCES "users" ("Id"),
            CONSTRAINT "FK_messages_messages_ReplyToMessageId" FOREIGN KEY ("ReplyToMessageId") REFERENCES "messages" ("Id") ON DELETE SET NULL
        );
        CREATE INDEX "IX_messages_ConversationId_CreatedAt_Id" ON "messages" ("ConversationId", "CreatedAt", "Id");
        CREATE INDEX "IX_messages_ReplyToMessageId" ON "messages" ("ReplyToMessageId");

        CREATE TABLE "password_reset_codes" (
            "Id" uuid NOT NULL,
            "UserId" uuid NOT NULL,
            "CodeHash" character varying(60) NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
            "ExpiresAt" timestamp with time zone NOT NULL DEFAULT now(),
            "UsedAt" timestamp with time zone NULL,
            "Attempts" integer NOT NULL DEFAULT 0,
            CONSTRAINT "PK_password_reset_codes" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_password_reset_codes_users_UserId" FOREIGN KEY ("UserId") REFERENCES "users" ("Id") ON DELETE CASCADE
        );
        CREATE INDEX "IX_password_reset_codes_UserId_ExpiresAt" ON "password_reset_codes" ("UserId", "ExpiresAt");
        """;
}