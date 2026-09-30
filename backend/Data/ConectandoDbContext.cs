using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Data;

public class ConectandoDbContext(DbContextOptions<ConectandoDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<Block> Blocks => Set<Block>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostMedia> PostMedia => Set<PostMedia>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<PostSave> PostSaves => Set<PostSave>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Share> Shares => Set<Share>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMember> ConversationMembers => Set<ConversationMember>();
    public DbSet<Message> Messages => Set<Message>();
public DbSet<PasswordResetCode> PasswordResetCodes => Set<PasswordResetCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConectandoDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}