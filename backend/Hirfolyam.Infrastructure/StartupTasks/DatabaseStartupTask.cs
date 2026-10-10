using Hirfolyam.Infrastructure.Options;
using Hirfolyam.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hirfolyam.Infrastructure.StartupTasks;

public sealed class DatabaseStartupTask(
    NewsDbContext database,
    NewsSourceSynchronizer synchronizer,
    IOptions<DatabaseOptions> options)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (options.Value.MigrateOnStartup)
        {
            await database.Database.MigrateAsync(cancellationToken);
        }

        await synchronizer.SynchronizeAsync(cancellationToken);
    }
}
