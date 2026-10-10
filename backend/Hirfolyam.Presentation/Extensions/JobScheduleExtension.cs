using Hirfolyam.Infrastructure.Options;
using Hirfolyam.Presentation.Jobs;
using Hirfolyam.Presentation.Options;
using Quartz;

namespace Hirfolyam.Presentation.Extensions;

public static class JobScheduleExtension
{
    // Gives the startup import time to finish before the first hot topic run.
    private static readonly TimeSpan HotTopicsStartupDelay = TimeSpan.FromMinutes(2);

    public static IServiceCollection AddJobs(this IServiceCollection services, IConfiguration configuration)
    {
        var import = JobScheduleOptions.Read(configuration, JobScheduleOptions.ImportSection);
        var hotTopics = JobScheduleOptions.Read(configuration, JobScheduleOptions.HotTopicsSection);
        var hotTopicsEnabled = hotTopics.Enabled && HasOpenRouterKey(configuration);

        if (!import.Enabled && !hotTopicsEnabled)
        {
            return services;
        }

        services.AddQuartz(scheduler =>
        {
            if (import.Enabled)
            {
                scheduler.AddImportJob(import);
            }

            if (hotTopicsEnabled)
            {
                scheduler.AddHotTopicsJob(hotTopics);
            }
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        return services;
    }

    public static WebApplication LogSkippedJobs(this WebApplication app)
    {
        if (HasOpenRouterKey(app.Configuration))
        {
            return app;
        }

        var hotTopics = JobScheduleOptions.Read(app.Configuration, JobScheduleOptions.HotTopicsSection);

        if (hotTopics.Enabled)
        {
            app.Logger.LogWarning("Hot topics are not scheduled because OpenRouter:ApiKey is missing");
        }

        return app;
    }

    // Without an API key the hot topic job has nothing to do.
    private static bool HasOpenRouterKey(IConfiguration configuration)
    {
        var options = configuration.GetSection(OpenRouterOptions.SectionName).Get<OpenRouterOptions>();

        return !string.IsNullOrWhiteSpace(options?.ApiKey);
    }

    private static void AddImportJob(this IServiceCollectionQuartzConfigurator scheduler, JobScheduleOptions options)
    {
        var jobKey = new JobKey("article-import");

        scheduler.AddJob<ImportArticlesJob>(job => job.WithIdentity(jobKey));
        scheduler.AddTrigger(trigger => trigger
            .WithIdentity("article-import-schedule")
            .ForJob(jobKey)
            .WithCronSchedule(options.CronExpression, cron => cron
                .WithMisfireHandlingInstructionFireAndProceed()));

        if (options.RunOnStartup)
        {
            scheduler.AddTrigger(trigger => trigger
                .WithIdentity("article-import-startup")
                .ForJob(jobKey)
                .StartNow());
        }
    }

    private static void AddHotTopicsJob(this IServiceCollectionQuartzConfigurator scheduler, JobScheduleOptions options)
    {
        var jobKey = new JobKey("hot-topics");

        scheduler.AddJob<GenerateHotTopicsJob>(job => job.WithIdentity(jobKey));
        // A missed run is skipped; the next one summarizes the newest articles anyway.
        scheduler.AddTrigger(trigger => trigger
            .WithIdentity("hot-topics-schedule")
            .ForJob(jobKey)
            .WithCronSchedule(options.CronExpression, cron => cron
                .WithMisfireHandlingInstructionDoNothing()));

        if (options.RunOnStartup)
        {
            scheduler.AddTrigger(trigger => trigger
                .WithIdentity("hot-topics-startup")
                .ForJob(jobKey)
                .StartAt(DateTimeOffset.UtcNow.Add(HotTopicsStartupDelay)));
        }
    }
}
