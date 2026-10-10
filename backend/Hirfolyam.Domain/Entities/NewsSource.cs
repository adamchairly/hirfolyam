namespace Hirfolyam.Domain.Entities;

// A stored copy of a news source definition, kept for queries and article references.
public sealed class NewsSource
{
    public int Id { get; set; }

    public required string Key { get; set; }

    public required string Name { get; set; }

    public required string BaseUrl { get; set; }

    public string? IconUrl { get; set; }

    public bool IsEnabled { get; set; }

    public DateTimeOffset? LastImportedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
