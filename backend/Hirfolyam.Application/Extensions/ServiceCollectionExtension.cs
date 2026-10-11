using FluentValidation;
using Hirfolyam.Application.Behaviors;
using Hirfolyam.Application.Features;
using Hirfolyam.Application.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hirfolyam.Application.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(ServiceCollectionExtension).Assembly;

        services.AddMediatR(mediator => mediator
            .RegisterServicesFromAssembly(assembly)
            .AddOpenBehavior(typeof(ValidationBehavior<,>)));
        services.AddValidatorsFromAssembly(assembly);

        services.AddOptions<ImportOptions>()
            .Bind(configuration.GetSection(ImportOptions.SectionName))
            .Validate(options => options.LookbackHours is >= 1 and <= 720, "Import:LookbackHours must be from 1 to 720.")
            .ValidateOnStart();

        services.AddOptions<HotTopicsOptions>()
            .Bind(configuration.GetSection(HotTopicsOptions.SectionName))
            .Validate(options => options.ArticleCount is >= 10 and <= 500, "HotTopics:ArticleCount must be from 10 to 500.")
            .Validate(options => options.TopicCount is >= 1 and <= 10, "HotTopics:TopicCount must be from 1 to 10.")
            .Validate(options => options.MaxAgeHours is >= 1 and <= 48, "HotTopics:MaxAgeHours must be from 1 to 48.")
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton(_ => NewsSourceCatalog.Load());
        services.AddSingleton<CategoryMapper>();
        services.AddScoped<ArticleImporter>();

        return services;
    }
}
