using AutoEdge.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoEdge.Tests.TestSupport;

internal static class TestDbFactory
{
    public static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"AutoEdge.Tests.{Guid.NewGuid()}")
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
