using Hirfolyam.Domain.Catalogs;

namespace Hirfolyam.Application.Features;

public sealed class CategoryRule
{
    public string[] Feed { get; init; } = [];

    public string[] Paths { get; init; } = [];
}

public sealed class CategoryRules
{
    private static readonly HashSet<string> KnownSlugs = Categories.All
        .Select(category => category.Slug)
        .ToHashSet(StringComparer.Ordinal);

    private readonly Dictionary<string, string> feedCategories = new(StringComparer.Ordinal);
    private readonly HashSet<string> ignoredCategories;
    private readonly KeyValuePair<string, string>[] pathPrefixes;

    public CategoryRules(
        string? defaultSlug,
        IEnumerable<string> ignoredFeedCategories,
        IReadOnlyDictionary<string, CategoryRule> rules)
    {
        if (defaultSlug is not null && !KnownSlugs.Contains(defaultSlug))
        {
            throw new InvalidOperationException($"Unknown default category: {defaultSlug}.");
        }

        DefaultSlug = defaultSlug;
        ignoredCategories = ignoredFeedCategories
            .Select(CategoryNameNormalizer.Normalize)
            .ToHashSet(StringComparer.Ordinal);

        var prefixes = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (slug, rule) in rules)
        {
            if (!KnownSlugs.Contains(slug))
            {
                throw new InvalidOperationException($"Unknown category: {slug}.");
            }

            foreach (var name in rule.Feed)
            {
                Add(feedCategories, CategoryNameNormalizer.Normalize(name), slug);
            }

            foreach (var path in rule.Paths)
            {
                if (!path.StartsWith('/') || path.Contains('?') || path.Contains('#'))
                {
                    throw new InvalidOperationException($"Invalid category path: {path}.");
                }

                Add(prefixes, path.TrimEnd('/').ToLowerInvariant(), slug);
            }
        }

        // The most specific path wins.
        pathPrefixes = prefixes.OrderByDescending(prefix => prefix.Key.Length).ToArray();
    }

    public static CategoryRules Empty { get; } = new(null, [], new Dictionary<string, CategoryRule>());

    public string? DefaultSlug { get; }

    public string? MatchFeedCategory(string normalizedName)
    {
        return feedCategories.GetValueOrDefault(normalizedName);
    }

    public bool IsIgnored(string normalizedName)
    {
        return ignoredCategories.Contains(normalizedName);
    }

    // Matches complete path segments only, so /sport does not match /sportgazdasag.
    public string? MatchPath(string path)
    {
        var normalizedPath = path.ToLowerInvariant().TrimEnd('/');

        foreach (var (prefix, slug) in pathPrefixes)
        {
            if (normalizedPath == prefix || normalizedPath.StartsWith(prefix + "/", StringComparison.Ordinal))
            {
                return slug;
            }
        }

        return null;
    }

    private static void Add(Dictionary<string, string> mappings, string name, string slug)
    {
        if (mappings.TryGetValue(name, out var existing) && existing != slug)
        {
            throw new InvalidOperationException($"'{name}' maps to both {existing} and {slug}.");
        }

        mappings[name] = slug;
    }
}
