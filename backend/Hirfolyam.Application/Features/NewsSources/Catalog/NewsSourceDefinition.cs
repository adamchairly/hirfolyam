namespace Hirfolyam.Application.Features;

public sealed class NewsSourceDefinition
{
    public required string Key { get; init; }

    public required string Name { get; init; }

    public required string BaseUrl { get; init; }

    public string? IconUrl { get; init; }

    public required int CategoryId { get; init; }

    public bool Enabled { get; init; } = true;

    public IReadOnlyList<string> Feeds { get; init; } = [];

    public CategoryRules CategoryRules { get; init; } = CategoryRules.Empty;
}
