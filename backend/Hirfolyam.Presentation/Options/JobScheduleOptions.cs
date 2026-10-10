
namespace Hirfolyam.Presentation.Options;

public sealed class JobScheduleOptions
{
    public const string ImportSection = "Jobs:Import";
    public const string HotTopicsSection = "Jobs:HotTopics";

    public bool Enabled { get; set; } = true;

    public bool RunOnStartup { get; set; }

    public string CronExpression { get; set; } = "";

    public static JobScheduleOptions Read(IConfiguration configuration, string section)
    {
        var options = configuration.GetSection(section).Get<JobScheduleOptions>() ?? new JobScheduleOptions();

        if (options.Enabled && !Quartz.CronExpression.IsValidExpression(options.CronExpression))
        {
            throw new InvalidOperationException($"{section}:CronExpression is invalid.");
        }

        return options;
    }
}
