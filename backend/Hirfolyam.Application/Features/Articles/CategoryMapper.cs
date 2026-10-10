using Hirfolyam.Domain.Catalogs;

namespace Hirfolyam.Application.Features;

public sealed class CategoryMapper(NewsSourceCatalog catalog)
{
    private static readonly Dictionary<string, int> CategoryIdsBySlug = Categories.All
        .ToDictionary(category => category.Slug, category => category.Id, StringComparer.Ordinal);

    // Feed categories that literally name a category, for example "Sport" or "Belföld".
    private static readonly Dictionary<string, int> CategoryIdsByName = Categories.All
        .SelectMany(category => new[] { category.Name, category.Slug }
            .Select(name => (Name: CategoryNameNormalizer.Normalize(name), category.Id)))
        .DistinctBy(entry => entry.Name)
        .ToDictionary(entry => entry.Name, entry => entry.Id, StringComparer.Ordinal);

    public int? Map(string sourceKey, IEnumerable<string> feedCategories, string? articleUrl = null)
    {
        var source = catalog.Find(sourceKey);
        var rules = source?.CategoryRules ?? CategoryRules.Empty;

        // The URL section is the most reliable signal.
        if (source is not null && TryGetSourcePath(source, articleUrl, out var path)
            && rules.MatchPath(path) is { } pathSlug)
        {
            return CategoryIdsBySlug[pathSlug];
        }

        var names = feedCategories
            .Select(CategoryNameNormalizer.Normalize)
            .Where(name => name.Length > 0)
            .ToArray();

        // Source rules take priority over general category names.
        foreach (var name in names)
        {
            if (rules.MatchFeedCategory(name) is { } slug)
            {
                return CategoryIdsBySlug[slug];
            }
        }

        foreach (var name in names)
        {
            if (!catalog.IsIgnored(name) && !rules.IsIgnored(name)
                && CategoryIdsByName.TryGetValue(name, out var categoryId))
            {
                return categoryId;
            }
        }

        return rules.DefaultSlug is { } defaultSlug ? CategoryIdsBySlug[defaultSlug] : null;
    }

    private static bool TryGetSourcePath(NewsSourceDefinition source, string? articleUrl, out string path)
    {
        path = "";

        if (!Uri.TryCreate(source.BaseUrl, UriKind.Absolute, out var sourceUri)
            || !Uri.TryCreate(articleUrl, UriKind.Absolute, out var articleUri)
            || articleUri.Scheme is not ("http" or "https")
            || !string.Equals(Host(sourceUri), Host(articleUri), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        path = Uri.UnescapeDataString(articleUri.AbsolutePath);

        return true;
    }

    private static string Host(Uri uri)
    {
        return uri.IdnHost.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
            ? uri.IdnHost[4..]
            : uri.IdnHost;
    }
}
