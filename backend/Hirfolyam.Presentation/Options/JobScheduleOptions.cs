namespace Hirfolyam.Presentation.Options;

public sealed class JobScheduleOptions
{
    public bool Enabled { get; set; } = true;

    public bool RunOnStartup { get; set; }

    public string CronExpression { get; set; } = "";
}
