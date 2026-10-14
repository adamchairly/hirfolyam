using Hirfolyam.Application.Shared;
using Hirfolyam.Presentation.Jobs;
using Hirfolyam.Presentation.Options;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Serilog;

namespace Hirfolyam.Presentation.Extensions;

public static class ServiceCollectionExtension
{
    // Gives the startup import time to finish before the first hot topic run.
    private static readonly TimeSpan HotTopicsStartupDelay = TimeSpan.FromMinutes(2);

    // Gives the startup hot topic run time to finish, so the day includes its topics.
    private static readonly TimeSpan DailyTopicsStartupDelay = TimeSpan.FromMinutes(5);

    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(new ErrorResponse
            {
                Error = ErrorCodes.BadRequest,
                Message = string.Join("; ", context.ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage)),
                Code = 400
            });
        });

        services.AddOpenApi();

        services.AddSerilog((provider, logging) => logging
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(provider)
            .Enrich.FromLogContext()
            .WriteToApplicationConsole());

        services.AddJobs(configuration);

        return services;
    }

    private static void AddJobs(this IServiceCollection services, IConfiguration configuration)
    {
        var jobs = configuration.GetSection(JobsOptions.SectionName).Get<JobsOptions>() ?? new JobsOptions();

        services.AddQuartz(scheduler =>
        {
            if (jobs.Import.Enabled)
            {
                scheduler.AddScheduledJob<ImportArticlesJob>("article-import", jobs.Import, TimeSpan.Zero);
            }

            if (jobs.HotTopics.Enabled)
            {
                scheduler.AddScheduledJob<GenerateHotTopicsJob>("hot-topics", jobs.HotTopics, HotTopicsStartupDelay);
            }

            if (jobs.DailyTopics.Enabled)
            {
                scheduler.AddScheduledJob<GenerateDailyTopicsJob>("daily-topics", jobs.DailyTopics, DailyTopicsStartupDelay);
            }
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
    }

    private static void AddScheduledJob<TJob>(
        this IServiceCollectionQuartzConfigurator scheduler,
        string name,
        JobScheduleOptions options,
        TimeSpan startupDelay) where TJob : IJob
    {
        // Quartz itself rejects the expression only when the scheduler starts.
        if (!CronExpression.IsValidExpression(options.CronExpression))
        {
            throw new InvalidOperationException($"The cron expression of the {name} job is invalid: '{options.CronExpression}'.");
        }

        var jobKey = new JobKey(name);

        scheduler.AddJob<TJob>(job => job.WithIdentity(jobKey));
        scheduler.AddTrigger(trigger => trigger
            .WithIdentity($"{name}-schedule")
            .ForJob(jobKey)
            .WithCronSchedule(options.CronExpression));

        if (options.RunOnStartup)
        {
            scheduler.AddTrigger(trigger => trigger
                .WithIdentity($"{name}-startup")
                .ForJob(jobKey)
                .StartAt(DateTimeOffset.UtcNow.Add(startupDelay)));
        }
    }
}
