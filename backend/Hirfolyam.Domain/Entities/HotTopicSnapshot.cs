namespace Hirfolyam.Domain.Entities;

// One hot topic run. Snapshots are kept, so the topics of a whole day can be summarized later.
public sealed class HotTopicSnapshot
{
    public long Id { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public required string Model { get; set; }

    // The number of newest articles the topics were chosen from.
    public int ArticleCount { get; set; }

    public int? InputTokens { get; set; }

    public int? OutputTokens { get; set; }

    public decimal? CostUsd { get; set; }

    public List<HotTopic> Topics { get; set; } = [];
}
