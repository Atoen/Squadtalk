using Microsoft.EntityFrameworkCore;
using Squadtalk.Server.Models;

namespace Tests;

public sealed class DbFixture : IDisposable
{
    public AppDbContext DbContext { get; }

    public void Dispose()
    {
        DbContext.Dispose();
    }

    public DbFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        DbContext = new AppDbContext(options);
    }
}