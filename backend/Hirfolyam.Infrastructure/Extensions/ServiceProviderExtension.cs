using Hirfolyam.Application.Features;
using Hirfolyam.Infrastructure.Options;
using Hirfolyam.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Hirfolyam.Infrastructure.Extensions;

public static class ServiceProviderExtension
{
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;

        if (provider.GetRequiredService<IOptions<DatabaseOptions>>().Value.MigrateOnStartup)
        {
            await provider.GetRequiredService<NewsDbContext>().Database.MigrateAsync(cancellationToken);
        }

        await provider.GetRequiredService<ISender>().Send(new SyncNewsSourcesCommand(), cancellationToken);
    }
}
