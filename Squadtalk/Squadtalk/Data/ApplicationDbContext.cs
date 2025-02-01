using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Data.TypedIds;
using Squadtalk.Data.Entities;
using Squadtalk.Data.Sql;
using Squadtalk.Data.TypedIds;

namespace Squadtalk.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<UserId>, UserId>
{
    private readonly ILogger<ApplicationDbContext> _logger;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ILogger<ApplicationDbContext> logger) : base(options)
    {
        _logger = logger;
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    // public DbSet<ChatUser> ChatUsers { get; set; } = default!;

    public DbSet<Message> Messages { get; set; } = default!;

    public DbSet<Group> Channels { get; set; } = default!;

    // public DbSet<GroupParticipant> GroupParticipants { get; set; } = default!;

    public DbSet<DbFile> Files { get; set; } = default!;

    public DbSet<FriendRequest> FriendRequests { get; set; } = default!;

    public DbSet<Friendship> Friendships { get; set; } = default!;

    public IQueryable<FriendRequestOutput> AddFriendRequest(Guid senderId, string recipientUsername) =>
        FromExpression(() => AddFriendRequest(senderId, recipientUsername));

    public IQueryable<FriendRequestResponseOutput> RespondToFriendRequest(Guid acceptingId, int requestId, bool accepted) =>
        FromExpression(() => RespondToFriendRequest(acceptingId, requestId, accepted));

    public IQueryable<RemoveFriendOutput> RemoveFriend(Guid removingUserId, Guid friendId) =>
        FromExpression(() => RemoveFriend(removingUserId, friendId));

    protected override void OnModelCreating(ModelBuilder builder)
    {
        _logger.LogInformation("Creating model...");

        base.OnModelCreating(builder);

        builder.HasDbFunction(() => AddFriendRequest(default, default!))
            .HasName("send_friend_request");

        builder.HasDbFunction(() => RespondToFriendRequest(default, default, default))
            .HasName("respond_to_friend_request");

        builder.HasDbFunction(() => RemoveFriend(default, default))
            .HasName("remove_friend");

        // var userIdConverter = new ValueConverter<UserId, Guid>(
        //     x => x.Value,
        //     x => new UserId(x));
        //
        // var groupIdConverter = new ValueConverter<GroupId, string>(
        //     x => x.Value,
        //     x => new GroupId(x));

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("AspNetUsers");
            entity.Property(x => x.Id)
                // .HasConversion(userIdConverter)
                .ValueGeneratedOnAdd();

            entity.Property(x => x.UserName)
                .HasColumnName("UserName");
        });

        // builder.Entity<ChatUser>(entity =>
        // {
        //     entity.ToTable("AspNetUsers");
        //     entity.HasOne<ApplicationUser>()
        //         .WithOne()
        //         .HasForeignKey<ChatUser>(x => x.Id);
        //
        //     entity.Property(x => x.Username)
        //         .HasColumnName("UserName");
        //
        //     entity.Property(x => x.LastSeen)
        //         .HasDefaultValueSql("NOW()");
        // });

        builder.Entity<Group>(entity =>
        {
            // entity.Property(x => x.Id)
                // .HasConversion(groupIdConverter)
                // .HasMaxLength(32);

            entity.HasOne(x => x.LastMessage);
            entity.Navigation(x => x.LastMessage)
                .AutoInclude();
        });

        builder.Entity<GroupParticipant>(entity =>
        {
            entity.HasKey(x => new { x.UserId, x.GroupId });

            // entity.Property(x => x.GroupId)
            //     .HasConversion(groupIdConverter);
            //
            // entity.Property(x => x.UserId)
            //     .HasConversion(userIdConverter);

            entity.HasOne(x => x.User)
                .WithMany(x => x.GroupParticipants)
                .HasForeignKey(x => x.UserId);

            entity.HasOne(x => x.Group)
                .WithMany(x => x.Participants)
                .HasForeignKey(x => x.GroupId);

            entity.Property(x => x.JoinedAt)
                .HasDefaultValueSql("NOW()");
        });

        builder.Entity<FriendRequest>()
            .Property(x => x.Id)
            // .HasConversion(id => id.Value, value => new FriendRequestId(value))
            .ValueGeneratedOnAdd();

        // builder.Entity<DbFile>()
        //     .Property(x => x.GroupId)
        //     .HasConversion(id => id.Value, value => new GroupId(value));
        //
        // builder.Entity<DbFile>()
        //     .Property(x => x.TusId)
        //     .HasConversion(id => id.Value, value => new TusFileId(value));

        // builder.Entity<IdentityRole<UserId>>()
        //     .Property(x => x.Id)
        //     .HasConversion(userIdConverter);
        //
        // builder.Entity<Message>()
        //     .Property(x => x.GroupId)
        //     .HasConversion(groupIdConverter);

        builder.Entity<Message>()
            .HasIndex(x => x.GroupId);

        _logger.LogInformation("done creating model");
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<UserId>()
            .HaveConversion<UserIdConverter>();

        configurationBuilder
            .Properties<GroupId>()
            .HaveConversion<GroupIdConverter>()
            .HaveMaxLength(32);

        configurationBuilder
            .Properties<FriendRequestId>()
            .HaveConversion<FriendRequestIdConverter>();

        configurationBuilder
            .Properties<TusFileId>()
            .HaveConversion<TusFileIdConverter>()
            .HaveMaxLength(40);
    }
}
