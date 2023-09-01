using Microsoft.EntityFrameworkCore;
using Squadtalk.Server.Models;

namespace Tests;

public class DbFixture : IDisposable
{
    public AppDbContext DbContext { get; set; }

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