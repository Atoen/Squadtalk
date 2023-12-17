using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Squadtalk.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
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
        
        builder.Entity<Channel>()
            .HasMany(x => x.Participants)
            .WithMany(x => x.Channels);
        
        builder.Entity<ApplicationUser>()
            .Navigation(x => x.Channels)
            .AutoInclude(false);
    }
}