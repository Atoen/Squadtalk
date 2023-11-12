using Microsoft.EntityFrameworkCore;

namespace Squadtalk.Server.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        Options = options;
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public DbContextOptions<AppDbContext> Options { get; }

    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Message> Messages { get; set; } = default!;
    public DbSet<Channel> Channels { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Navigation(x => x.RefreshTokens)
            .AutoInclude(false);

        modelBuilder.Entity<Channel>()
            .HasMany(x => x.Participants)
            .WithMany(x => x.Channels);


        modelBuilder.Entity<User>()
            .Navigation(x => x.Channels)
            .AutoInclude(false);
    }
}