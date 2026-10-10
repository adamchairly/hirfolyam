namespace Hirfolyam.Domain.Entities;

public sealed class Article
{
    public long Id { get; set; }

    public int NewsSourceId { get; set; }

    public NewsSource NewsSource { get; set; } = null!;

    public int? CategoryId { get; set; }

    public Category? Category { get; set; }

    public required string ExternalId { get; set; }

    public required string Url { get; set; }

    public required string Title { get; set; }

    public string? Lead { get; set; }

    public string? LeadSearchText { get; set; }

    public string? Content { get; set; }

    public string? Author { get; set; }

    public string? ImageUrl { get; set; }

    public DateTimeOffset PublishedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public required string ExternalIdHash { get; set; }

    public required string UrlHash { get; set; }
}
