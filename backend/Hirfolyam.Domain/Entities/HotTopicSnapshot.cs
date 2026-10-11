namespace Hirfolyam.Domain.Entities;

// Snapshots are kept, so the topics of a whole day can be summarized
public sealed class HotTopicSnapshot
{
    public long Id { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public required string Model { get; set; }

    public int ArticleCount { get; set; }

    public int? InputTokens { get; set; }

    public int? OutputTokens { get; set; }

    public decimal? CostUsd { get; set; }

    public List<HotTopic> Topics { get; set; } = [];
}
