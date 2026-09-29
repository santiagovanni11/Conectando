namespace Conectando.Api.Tests.TestInfrastructure;

public static partial class TestSchema
{
    /// <summary>Usuarios y sus vínculos: solicitudes, amistades, follows y bloqueos.</summary>
    private const string SocialTablesSql = """
        CREATE TABLE "users" (
            "Id" uuid NOT NULL,
            "UserName" character varying(50) NOT NULL,
            "Email" character varying(200) NOT NULL,
            "PasswordHash" text NOT NULL,
            "DisplayName" character varying(50) NOT NULL,
            "Bio" text NULL,
            "ProfileImageUrl" text NULL,
            "ProfileImagePublicId" text NULL,
            "ProfileImageSizeBytes" bigint NOT NULL DEFAULT 0,
            "ProfileImageZoom" double precision NOT NULL DEFAULT 1,
            "ProfileImageOffsetX" integer NOT NULL DEFAULT 0,
            "ProfileImageOffsetY" integer NOT NULL DEFAULT 0,
            "IsPrivate" boolean NOT NULL DEFAULT false,
            "CreatedAt" timestamp with time zone NOT NULL,
            "UpdatedAt" timestamp with time zone NOT NULL,
            "SecurityStamp" uuid NOT NULL DEFAULT gen_random_uuid(),
            "DeletedAt" timestamp with time zone NULL,
            CONSTRAINT "PK_users" PRIMARY KEY ("Id")
        );
        CREATE TABLE "friend_requests" (
            "RequesterId" uuid NOT NULL,
            "AddresseeId" uuid NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
            CONSTRAINT "PK_friend_requests" PRIMARY KEY ("RequesterId", "AddresseeId"),
            CONSTRAINT "CK_friend_requests_no_self" CHECK ("RequesterId" <> "AddresseeId"),
            CONSTRAINT "FK_friend_requests_requester" FOREIGN KEY ("RequesterId") REFERENCES "users" ("Id"),
            CONSTRAINT "FK_friend_requests_addressee" FOREIGN KEY ("AddresseeId") REFERENCES "users" ("Id")
        );
        CREATE TABLE "friendships" (
            "UserLowId" uuid NOT NULL,
            "UserHighId" uuid NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
            CONSTRAINT "PK_friendships" PRIMARY KEY ("UserLowId", "UserHighId"),
            CONSTRAINT "CK_friendships_normalized" CHECK ("UserLowId" < "UserHighId"),
            CONSTRAINT "FK_friendships_low" FOREIGN KEY ("UserLowId") REFERENCES "users" ("Id"),
            CONSTRAINT "FK_friendships_high" FOREIGN KEY ("UserHighId") REFERENCES "users" ("Id")
        );
        CREATE TABLE "follows" (
            "UserId" uuid NOT NULL,
            "TargetUserId" uuid NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
            CONSTRAINT "PK_follows" PRIMARY KEY ("UserId", "TargetUserId"),
            CONSTRAINT "CK_follows_no_self" CHECK ("UserId" <> "TargetUserId"),
            CONSTRAINT "FK_follows_user" FOREIGN KEY ("UserId") REFERENCES "users" ("Id"),
            CONSTRAINT "FK_follows_target" FOREIGN KEY ("TargetUserId") REFERENCES "users" ("Id")
        );
        CREATE TABLE "blocks" (
            "UserId" uuid NOT NULL,
            "BlockedUserId" uuid NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
            CONSTRAINT "PK_blocks" PRIMARY KEY ("UserId", "BlockedUserId"),
            CONSTRAINT "CK_blocks_no_self" CHECK ("UserId" <> "BlockedUserId"),
            CONSTRAINT "FK_blocks_user" FOREIGN KEY ("UserId") REFERENCES "users" ("Id"),
            CONSTRAINT "FK_blocks_blocked" FOREIGN KEY ("BlockedUserId") REFERENCES "users" ("Id")
        );
        CREATE TABLE "reports" (
            "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
            "ReporterId" uuid NOT NULL,
            "TargetUserId" uuid NOT NULL,
            "Reason" character varying(32) NOT NULL,
            "Details" character varying(1000) NULL,
            "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
            CONSTRAINT "PK_reports" PRIMARY KEY ("Id"),
            CONSTRAINT "CK_reports_no_self" CHECK ("ReporterId" <> "TargetUserId"),
            CONSTRAINT "FK_reports_reporter" FOREIGN KEY ("ReporterId") REFERENCES "users" ("Id"),
            CONSTRAINT "FK_reports_target" FOREIGN KEY ("TargetUserId") REFERENCES "users" ("Id")
        );
        CREATE INDEX "IX_friend_requests_AddresseeId" ON "friend_requests" ("AddresseeId");
        CREATE INDEX "IX_friendships_UserHighId" ON "friendships" ("UserHighId");
        CREATE INDEX "IX_follows_TargetUserId" ON "follows" ("TargetUserId");
        CREATE INDEX "IX_blocks_BlockedUserId" ON "blocks" ("BlockedUserId");
        CREATE UNIQUE INDEX "IX_reports_ReporterId_TargetUserId" ON "reports" ("ReporterId", "TargetUserId");
        CREATE INDEX "IX_reports_TargetUserId" ON "reports" ("TargetUserId");
        """;
}