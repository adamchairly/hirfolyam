using Hirfolyam.Infrastructure.StartupTasks;
using Microsoft.Extensions.DependencyInjection;

namespace Hirfolyam.Infrastructure.Extensions;

public static class ServiceProviderExtension
{
    public static async Task InitializeInfrastructureAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();

        await scope.ServiceProvider.GetRequiredService<DatabaseStartupTask>().ExecuteAsync(cancellationToken);
    }
}
