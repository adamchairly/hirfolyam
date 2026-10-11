namespace Hirfolyam.Domain.Entities;

public sealed class HotTopic
{
    public const int TitleMaxLength = 200;

    public const int SummaryMaxLength = 500;

    public const int MinSources = 2;

    public long Id { get; set; }

    public long SnapshotId { get; set; }

    public HotTopicSnapshot Snapshot { get; set; } = null!;

    public int Rank { get; set; }

    public required string Title { get; set; }

    public required string Summary { get; set; }

    public required long[] ArticleIds { get; set; }
}
