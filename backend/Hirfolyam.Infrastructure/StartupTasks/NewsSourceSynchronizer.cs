using Hirfolyam.Application.Features;
using Hirfolyam.Domain.Entities;
using Hirfolyam.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hirfolyam.Infrastructure.StartupTasks;

public sealed record NewsSourceSyncResult(int Added, int Updated, int Disabled);

// Copies the source definitions into the database
// Sources removed from the catalog are disabled
public sealed class NewsSourceSynchronizer(
    NewsDbContext database,
    NewsSourceCatalog catalog,
    TimeProvider timeProvider,
    ILogger<NewsSourceSynchronizer> logger)
{
    public async Task<NewsSourceSyncResult> SynchronizeAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var stored = await database.NewsSources.ToDictionaryAsync(source => source.Key, cancellationToken);
        var added = 0;
        var updated = 0;
        var disabled = 0;

        foreach (var definition in catalog.Sources)
        {
            if (!stored.TryGetValue(definition.Key, out var source))
            {
                database.NewsSources.Add(new NewsSource
                {
                    Key = definition.Key,
                    Name = definition.Name,
                    BaseUrl = definition.BaseUrl,
                    IconUrl = definition.IconUrl,
                    CategoryId = definition.CategoryId,
                    IsEnabled = definition.Enabled,
                    CreatedAt = now,
                    UpdatedAt = now
                });
                added++;

                continue;
            }

            if (source.Name == definition.Name
                && source.BaseUrl == definition.BaseUrl
                && source.IconUrl == definition.IconUrl
                && source.CategoryId == definition.CategoryId
                && source.IsEnabled == definition.Enabled)
            {
                continue;
            }

            source.Name = definition.Name;
            source.BaseUrl = definition.BaseUrl;
            source.IconUrl = definition.IconUrl;
            source.CategoryId = definition.CategoryId;
            source.IsEnabled = definition.Enabled;
            source.UpdatedAt = now;
            updated++;
        }

        // Removed sources keep their articles, so they are only disabled.
        foreach (var source in stored.Values.Where(source => source.IsEnabled && catalog.Find(source.Key) is null))
        {
            source.IsEnabled = false;
            source.UpdatedAt = now;
            disabled++;
        }

        await database.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "News sources synchronized: {Added} added, {Updated} updated, {Disabled} disabled",
            added,
            updated,
            disabled);

        return new NewsSourceSyncResult(added, updated, disabled);
    }
}
