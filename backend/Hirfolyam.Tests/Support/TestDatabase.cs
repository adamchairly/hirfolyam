using Hirfolyam.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hirfolyam.Tests.Support;

internal static class TestDatabase
{
    // Contexts with the same name share their data, like separate scopes of one database.
    public static NewsDbContext Create(string databaseName)
    {
        var options = new DbContextOptionsBuilder<NewsDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new NewsDbContext(options);
    }
}
