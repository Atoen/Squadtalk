using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Data.TypedIds;
using Squadtalk.Data.Entities;
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
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var userConverter = new ValueConverter<UserId, Guid>(
        x => x.Value,
        x => new UserId(x));

        var channelConverter = new ValueConverter<ChannelId, string>(
        x => x.Value,
        x => new ChannelId(x));

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

        // base.OnModelCreating(builder);
        //
        // var userIdConverter = new ValueConverter<UserId, Guid>(
        //     x => x.Value,
        //     x => new UserId(x));
        //
        // var channelIdConverter = new ValueConverter<ChannelId, string>(
        //     x => x.Value,
        //     x => new ChannelId(x));
        //
        // builder.Entity<IdentityRole<UserId>>()
        //     .Property(x => x.Id)
        //     .HasConversion(userIdConverter);
        //
        // builder.Entity<Message>()
        //     .Property(x => x.ChannelId)
        //     .HasConversion(channelIdConverter);
        //
        // builder.Entity<DbFile>(entity =>
        // {
        //     entity.Property(x => x.ChannelId)
        //         .HasConversion(id => id.Value, value => new ChannelId(value));
        //
        //     entity.Property(x => x.TusId)
        //         .HasConversion(id => id.Value, value => new TusFileId(value));
        // });
        //
        // builder.Entity<ApplicationUser>(entity =>
        // {
        //     entity.Property(x => x.Id)
        //         .HasConversion(userIdConverter);
        //
        //     entity.Navigation(x => x.Channels)
        //         .AutoInclude(false);
        //
        //     entity.Navigation(x => x.Contacts)
        //         .AutoInclude(false);
        // });
        //
        // builder.Entity<Channel>(entity =>
        // {
        //     entity.Property(x => x.Id)
        //         .HasConversion(channelIdConverter);
        //
        //     entity.OwnsOne(x => x.LastMessage)
        //         .Property(x => x.AuthorId)
        //         .HasConversion(userIdConverter);
        //
        //     entity.OwnsOne(x => x.LastMessage)
        //         .Property(x => x.ChannelId)
        //         .HasConversion(channelIdConverter);
        //
        //     entity.HasMany(x => x.Participants)
        //         .WithMany(x => x.Channels);
        // });
    }
}
