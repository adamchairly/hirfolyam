namespace Hirfolyam.Presentation.Options;

public sealed class JobsOptions
{
    public const string SectionName = "Jobs";

    public JobScheduleOptions Import { get; set; } = new();

    public JobScheduleOptions HotTopics { get; set; } = new();
}
