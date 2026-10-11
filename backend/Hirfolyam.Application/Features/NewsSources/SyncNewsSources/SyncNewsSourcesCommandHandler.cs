using Hirfolyam.Application.Abstractions;
using Hirfolyam.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hirfolyam.Application.Features;

// Copies the source definitions into the database
// Sources removed from the catalog are disabled
internal sealed class SyncNewsSourcesCommandHandler(
    IApplicationDbContext context,
    NewsSourceCatalog catalog,
    TimeProvider timeProvider,
    ILogger<SyncNewsSourcesCommandHandler> logger) : IRequestHandler<SyncNewsSourcesCommand, SyncNewsSourcesResult>
{
    public async Task<SyncNewsSourcesResult> Handle(SyncNewsSourcesCommand request, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var stored = await context.NewsSources.ToDictionaryAsync(source => source.Key, cancellationToken);
        var added = 0;
        var updated = 0;
        var disabled = 0;

        foreach (var definition in catalog.Sources)
        {
            if (!stored.TryGetValue(definition.Key, out var source))
            {
                context.NewsSources.Add(new NewsSource
                {
                    Key = definition.Key,
                    Name = definition.Name,
                    BaseUrl = definition.BaseUrl,
                    IconUrl = definition.IconUrl,
                    CategoryId = definition.CategoryId,
                    IsEnabled = definition.IsEnabled,
                    CreatedAt = now,
                    UpdatedAt = now
                });
                added++;

                continue;
            }

            source.Name = definition.Name;
            source.BaseUrl = definition.BaseUrl;
            source.IconUrl = definition.IconUrl;
            source.CategoryId = definition.CategoryId;
            source.IsEnabled = definition.IsEnabled;

            // The change tracker reports a change only if a value differs.
            if (context.Entry(source).State == EntityState.Modified)
            {
                source.UpdatedAt = now;
                updated++;
            }
        }

        // Removed sources keep their articles, so they are only disabled.
        foreach (var source in stored.Values.Where(source => source.IsEnabled && catalog.Find(source.Key) is null))
        {
            source.IsEnabled = false;
            source.UpdatedAt = now;
            disabled++;
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "News sources synchronized: {Added} added, {Updated} updated, {Disabled} disabled",
            added,
            updated,
            disabled);

        return new SyncNewsSourcesResult(added, updated, disabled);
    }
}
