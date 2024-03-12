using System.Threading.Channels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Data;


namespace Squadtalk.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<UserId>, UserId>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public DbSet<Message> Messages { get; set; } = default!;

    public DbSet<Channel> Channels { get; set; } = default!;
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var userConverter = new ValueConverter<UserId, string>(
            x => x.Value,
            x => new UserId(x));

        var channelConverter = new ValueConverter<ChannelId, string>(
            x => x.Value,
            x => new ChannelId(x));
        
        builder.Entity<ApplicationUser>()
            .Property(x => x.Id)
            .HasConversion(userConverter);
        
        builder.Entity<ApplicationUser>()
            .Property(x => x.Id)
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