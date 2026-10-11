namespace Hirfolyam.Application.Features;

public sealed record RssArticle
{
    public string? ExternalId { get; init; }

    public required string Title { get; init; }

    public required string Url { get; init; }

    public required DateTimeOffset PublishedAt { get; init; }

    public string? Author { get; init; }

    public string? Description { get; init; }

    public string? Content { get; init; }

    public IReadOnlyCollection<string> Categories { get; init; } = [];

    public string? ImageUrl { get; init; }
}
