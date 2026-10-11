using System.Net;
using Hirfolyam.Application.Abstractions;
using Hirfolyam.Application.Features;
using Hirfolyam.Infrastructure.Analysis;
using Hirfolyam.Infrastructure.Feeds;
using Hirfolyam.Infrastructure.Options;
using Hirfolyam.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Hirfolyam.Infrastructure.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddNewsReaders();
        services.AddHotTopicGenerator(configuration);

        return services;
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("NewsDatabase")
            ?? throw new InvalidOperationException("ConnectionStrings:NewsDatabase is required.");

        services.AddDbContext<NewsDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<NewsDbContext>());

        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateOnStart();
    }

    private static void AddNewsReaders(this IServiceCollection services)
    {
        services.AddHttpClient<IRssFeedReader, RssFeedReader>(client =>
        {
            // Covers the whole download; the resilience handler only times the response headers.
            client.Timeout = TimeSpan.FromSeconds(90);
            client.MaxResponseContentBufferSize = RssFeedReader.MaximumResponseBytes;
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Hirfolyam/1.0");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/rss+xml, application/atom+xml, application/xml;q=0.9");
        })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                MaxAutomaticRedirections = 5,
                AutomaticDecompression = DecompressionMethods.All,
                ConnectTimeout = TimeSpan.FromSeconds(10),
                PooledConnectionLifetime = TimeSpan.FromMinutes(5)
            })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 2;
                options.Retry.Delay = TimeSpan.FromSeconds(1);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(20);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(70);
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
            });
    }

    private static void AddHotTopicGenerator(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OpenRouterOptions>()
            .Bind(configuration.GetSection(OpenRouterOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.HotTopics.Model), "OpenRouter:HotTopics:Model is required.")
            .Validate(options => options.HotTopics.MaxTokens is >= 1 and <= 100000, "OpenRouter:HotTopics:MaxTokens must be from 1 to 100000.")
            .Validate(options => options.HotTopics.TimeoutSeconds is >= 1 and <= 300, "OpenRouter:HotTopics:TimeoutSeconds must be from 1 to 300.")
            .ValidateOnStart();

        services.AddHttpClient<IHotTopicGenerator, OpenRouterHotTopicGenerator>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<OpenRouterOptions>>().Value;

            client.Timeout = TimeSpan.FromSeconds(options.HotTopics.TimeoutSeconds);
            client.MaxResponseContentBufferSize = 256 * 1024;
        })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                ConnectTimeout = TimeSpan.FromSeconds(10),
                PooledConnectionLifetime = TimeSpan.FromMinutes(5)
            });
    }
}
