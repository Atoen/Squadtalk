using Microsoft.EntityFrameworkCore;

namespace Squadtalk.Server.Models;

public class AppDbContext : DbContext
{
    public DbContextOptions<AppDbContext> Options { get; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        Options = options;
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Message> Messages { get; set; } = default!;
    public DbSet<Channel> Channels { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().Navigation(x => x.RefreshTokens)
            .AutoInclude(false);

        modelBuilder.Entity<User>().Navigation(x => x.Channels)
            .AutoInclude(false);
    }
}