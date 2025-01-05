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
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public DbSet<Message> Messages { get; set; } = default!;

    public DbSet<Channel> Channels { get; set; } = default!;

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

        var userConverter = new ValueConverter<UserId, Guid>(
            x => x.Value,
            x => new UserId(x));

        var channelConverter = new ValueConverter<ChannelId, string>(
            x => x.Value,
            x => new ChannelId(x));

        builder.Entity<FriendRequest>()
            .Property(x => x.Id)
            .HasConversion(id => id.Value, value => new FriendRequestId(value))
            .ValueGeneratedOnAdd();

        builder.Entity<DbFile>()
            .Property(x => x.ChannelId)
            .HasConversion(id => id.Value, value => new ChannelId(value));

        builder.Entity<DbFile>()
            .Property(x => x.TusId)
            .HasConversion(id => id.Value, value => new TusFileId(value));

        builder.Entity<ApplicationUser>()
            .Property(x => x.Id)
            .HasConversion(userConverter);

        builder.Entity<ApplicationUser>()
            .Property(x => x.Id)
            .HasConversion(x => x.Value, value => new UserId(value))
            .ValueGeneratedOnAdd();

        builder.Entity<IdentityRole<UserId>>()
            .Property(x => x.Id)
            .HasConversion(userConverter);

        builder.Entity<Message>()
            .Property(x => x.ChannelId)
            .HasConversion(channelConverter);

        builder.Entity<Channel>()
            .Property(x => x.Id)
            .HasConversion(channelConverter);

        builder.Entity<Channel>()
            .OwnsOne(x => x.LastMessage)
            .Property(x => x.AuthorId)
            .HasConversion(userConverter);

        builder.Entity<Channel>()
            .OwnsOne(x => x.LastMessage)
            .Property(x => x.ChannelId)
            .HasConversion(channelConverter);

        builder.Entity<Channel>()
            .HasMany(x => x.Participants)
            .WithMany(x => x.Channels);

        builder.Entity<ApplicationUser>()
            .Navigation(x => x.Channels)
            .AutoInclude(false);
    }
}
