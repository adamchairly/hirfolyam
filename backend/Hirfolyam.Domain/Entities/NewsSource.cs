namespace Hirfolyam.Domain.Entities;

public sealed class NewsSource
{
    public int Id { get; set; }

    public required string Key { get; set; }

    public required string Name { get; set; }

    public required string BaseUrl { get; set; }

    public string? IconUrl { get; set; }

    // The category that fits the source best
    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateTimeOffset? LastImportedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
