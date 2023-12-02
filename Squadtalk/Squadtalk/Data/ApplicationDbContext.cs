using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Squadtalk.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Message> Messages { get; set; } = default!;

    public DbSet<Channel> Channels { get; set; } = default!;

    // protected override void OnModelCreating(ModelBuilder builder)
    // {
    //     builder.Entity<Channel>()
    //         .HasMany(x => x.Participants)
    //         .WithMany(x => x.Channels);
    //     
    //     builder.Entity<ApplicationUser>()
    //         .Navigation(x => x.Channels)
    //         .AutoInclude(false);
    // }
}