using Microsoft.EntityFrameworkCore;
using Squadtalk.Server.Models;

namespace Tests;

public sealed class DbFixture : IDisposable
{
    public DbFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        DbContext = new AppDbContext(options);
    }

    public AppDbContext DbContext { get; }

    public void Dispose()
    {
        DbContext.Dispose();
    }
}