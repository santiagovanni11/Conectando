namespace Conectando.Api.Tests.TestInfrastructure;

public static partial class TestSchema
{
    /// <summary>Publicaciones, sus medios, comentarios y me gusta.</summary>
    private const string PostsTablesSql = """
        CREATE TABLE "posts" (
            "Id" uuid NOT NULL,
            "AuthorId" uuid NOT NULL,
            "Content" character varying(2000) NULL,
            "Privacy" integer NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL,
            "UpdatedAt" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_posts" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_posts_users_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "users" ("Id")
        );
        CREATE TABLE "post_media" (
            "Id" uuid NOT NULL,
            "PostId" uuid NULL,
            "UserId" uuid NOT NULL,
            "Url" character varying(2048) NOT NULL,
            "PublicId" character varying(256) NOT NULL,
            "DisplayOrder" integer NOT NULL,
            "State" integer NOT NULL,
            "SizeBytes" bigint NOT NULL DEFAULT 0,
            "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
            CONSTRAINT "PK_post_media" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_post_media_posts_PostId" FOREIGN KEY ("PostId") REFERENCES "posts" ("Id"),
            CONSTRAINT "FK_post_media_users_UserId" FOREIGN KEY ("UserId") REFERENCES "users" ("Id")
        );
        CREATE INDEX "IX_posts_AuthorId_CreatedAt" ON "posts" ("AuthorId", "CreatedAt");
        CREATE INDEX "IX_post_media_PostId_DisplayOrder" ON "post_media" ("PostId", "DisplayOrder");
        CREATE INDEX "IX_post_media_UserId_State" ON "post_media" ("UserId", "State");
        CREATE TABLE "comments" (
            "Id" uuid NOT NULL,
            "PostId" uuid NOT NULL,
            "AuthorId" uuid NOT NULL,
            "ParentCommentId" uuid NULL,
            "Content" character varying(1000) NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL,
            "UpdatedAt" timestamp with time zone NOT NULL,
            "DeletedAt" timestamp with time zone NULL,
            CONSTRAINT "PK_comments" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_comments_posts_PostId" FOREIGN KEY ("PostId") REFERENCES "posts" ("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_comments_users_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "users" ("Id"),
            CONSTRAINT "FK_comments_comments_ParentCommentId" FOREIGN KEY ("ParentCommentId") REFERENCES "comments" ("Id")
        );
        CREATE INDEX "IX_comments_PostId_CreatedAt_Id" ON "comments" ("PostId", "CreatedAt", "Id");
        CREATE INDEX "IX_comments_ParentCommentId_CreatedAt_Id" ON "comments" ("ParentCommentId", "CreatedAt", "Id");
        CREATE INDEX "IX_comments_AuthorId_CreatedAt" ON "comments" ("AuthorId", "CreatedAt");
        CREATE TABLE "likes" (
            "UserId" uuid NOT NULL,
            "PostId" uuid NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
            CONSTRAINT "PK_likes" PRIMARY KEY ("UserId", "PostId"),
            CONSTRAINT "FK_likes_posts_PostId" FOREIGN KEY ("PostId") REFERENCES "posts" ("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_likes_users_UserId" FOREIGN KEY ("UserId") REFERENCES "users" ("Id")
        );
        CREATE INDEX "IX_likes_PostId_UserId" ON "likes" ("PostId", "UserId");
        CREATE TABLE "post_saves" (
            "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
            "UserId" uuid NOT NULL,
            "PostId" uuid NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
            CONSTRAINT "PK_post_saves" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_post_saves_posts_PostId" FOREIGN KEY ("PostId") REFERENCES "posts" ("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_post_saves_users_UserId" FOREIGN KEY ("UserId") REFERENCES "users" ("Id") ON DELETE CASCADE
        );
        CREATE UNIQUE INDEX "IX_post_saves_UserId_PostId" ON "post_saves" ("UserId", "PostId");
        CREATE INDEX "IX_post_saves_UserId_CreatedAt" ON "post_saves" ("UserId", "CreatedAt");
        CREATE TABLE "shares" (
            "Id" uuid NOT NULL,
            "UserId" uuid NOT NULL,
            "PostId" uuid NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
            CONSTRAINT "PK_shares" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_shares_posts_PostId" FOREIGN KEY ("PostId") REFERENCES "posts" ("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_shares_users_UserId" FOREIGN KEY ("UserId") REFERENCES "users" ("Id")
        );
        CREATE INDEX "IX_shares_PostId_CreatedAt" ON "shares" ("PostId", "CreatedAt");
        CREATE INDEX "IX_shares_UserId_CreatedAt" ON "shares" ("UserId", "CreatedAt");
        """;
}