namespace Hirfolyam.Application.Options;

public sealed class HotTopicsOptions
{
    public const string SectionName = "HotTopics";

    // The newest articles the topics are chosen from.
    public int ArticleCount { get; set; } = 100;

    public int TopicCount { get; set; } = 5;

    // An older snapshot is no longer shown, so a failing job does not leave stale topics on the page.
    public int MaxAgeHours { get; set; } = 3;
}
