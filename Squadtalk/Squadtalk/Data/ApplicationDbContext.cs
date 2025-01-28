using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Data.TypedIds;
using Squadtalk.Data.Entities;
using Squadtalk.Data.Sql;
using Squadtalk.Data.TypedIds;
using FriendRequest = Squadtalk.Data.Entities.FriendRequest;

namespace Squadtalk.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<UserId>, UserId>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public DbSet<ChatUser> ChatUsers { get; set; } = default!;

    public DbSet<Message> Messages { get; set; } = default!;

    public DbSet<Group> Channels { get; set; } = default!;

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
        base.OnModelCreating(builder);

        builder.HasDbFunction(() => AddFriendRequest(default, default!))
            .HasName("send_friend_request");

        builder.HasDbFunction(() => RespondToFriendRequest(default, default, default))
            .HasName("respond_to_friend_request");

        builder.HasDbFunction(() => RemoveFriend(default, default))
            .HasName("remove_friend");

        var userIdConverter = new ValueConverter<UserId, Guid>(
            x => x.Value,
            x => new UserId(x));

        var groupIdConverter = new ValueConverter<GroupId, string>(
            x => x.Value,
            x => new GroupId(x));

        // builder.Entity<ChatUser>(entity =>
        // {
        //     entity.ToTable("AspNetUsers");
        //
        //     entity.Property(x => x.Id)
        //         .HasConversion(userIdConverter);
        // });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("AspNetUsers"); // Main table for ApplicationUser
            entity.Property(x => x.Id)
                .HasConversion(userIdConverter)
                .ValueGeneratedOnAdd();

            entity.Property(x => x.UserName)
                .HasColumnName("UserName")
                .IsRequired();
        });

        builder.Entity<ChatUser>(entity =>
        {
            entity.ToTable("AspNetUsers"); // Maps to the same table
            entity.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<ChatUser>(x => x.Id); // FK is the same as the PK in ApplicationUser

            entity.Property(x => x.Username)
                .HasColumnName("UserName")
                .IsRequired();
        });

        builder.Entity<FriendRequest>()
            .Property(x => x.Id)
            .HasConversion(id => id.Value, value => new FriendRequestId(value))
            .ValueGeneratedOnAdd();

        builder.Entity<DbFile>()
            .Property(x => x.GroupId)
            .HasConversion(id => id.Value, value => new GroupId(value));

        builder.Entity<DbFile>()
            .Property(x => x.TusId)
            .HasConversion(id => id.Value, value => new TusFileId(value));

        // builder.Entity<ApplicationUser>()
        //     .Property(x => x.Id)
        //     .HasConversion(userIdConverter)
        //     .ValueGeneratedOnAdd();

        builder.Entity<IdentityRole<UserId>>()
            .Property(x => x.Id)
            .HasConversion(userIdConverter);

        builder.Entity<Message>()
            .Property(x => x.GroupId)
            .HasConversion(groupIdConverter);

        builder.Entity<Message>()
            .HasIndex(x => x.GroupId);

        builder.Entity<Group>()
            .Property(x => x.Id)
            .HasConversion(groupIdConverter);

        builder.Entity<Group>()
            .OwnsOne(x => x.LastMessage)
            .Property(x => x.AuthorId)
            .HasConversion(userIdConverter);

        builder.Entity<Group>()
            .OwnsOne(x => x.LastMessage)
            .Property(x => x.GroupId)
            .HasConversion(groupIdConverter);

        builder.Entity<GroupParticipant>()
            .HasKey(x => new { x.UserId, x.GroupId });

        builder.Entity<GroupParticipant>()
            .Property(x => x.GroupId)
            .HasConversion(groupIdConverter);

        builder.Entity<GroupParticipant>()
            .Property(x => x.UserId)
            .HasConversion(userIdConverter);

        builder.Entity<GroupParticipant>()
            .HasOne(x => x.User)
            .WithMany(x => x.GroupParticipants)
            .HasForeignKey(x => x.UserId);

        builder.Entity<GroupParticipant>()
            .HasOne(x => x.Group)
            .WithMany(x => x.Participants)
            .HasForeignKey(x => x.GroupId);

        builder.Entity<GroupParticipant>()
            .Property(x => x.JoinedAt)
            .HasDefaultValueSql("NOW()");

        builder.Entity<ApplicationUser>()
            .Navigation(x => x.GroupParticipants)
            .AutoInclude(false);

        // builder.Entity<Group>()
        //     .HasMany(x => x.Participants);
        //     // .WithMany(x => x.Channels);
        //
        // builder.Entity<ApplicationUser>()
        //     .Navigation(x => x.Groups)
        //     .AutoInclude(false);
        //
        // builder.Entity<GroupParticipant>()
        //     .Property(x => x.UserId)
        //     .HasConversion(userIdConverter);
        //
        // builder.Entity<GroupParticipant>()
        //     .Property(x => x.GroupId)
        //     .HasConversion(groupIdConverter);
        //
        // builder.Entity<GroupParticipant>()
        //     .HasKey(x => new { x.UserId,
        //         ChannelId = x.GroupId });
    }
}
