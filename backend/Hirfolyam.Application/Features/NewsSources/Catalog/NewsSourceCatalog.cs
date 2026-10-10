using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Hirfolyam.Application.Features;

// The definition files are the single source of truth for news sources and their category rules.
public sealed partial class NewsSourceCatalog
{
    private const string ResourcePrefix = "NewsSources/";

    // Feed categories that never describe the topic, whatever the source.
    private static readonly string[] CommonIgnoredFeedCategories =
    [
        "Hír cikk a Google News-ba",
        "Hirdetés és disclaimer mentes",
        "Videó",
        "Video",
        "Podcast",
        "Galéria",
        "PR",
        "Hirdetés",
        "Szponzorált tartalom"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    private readonly Dictionary<string, NewsSourceDefinition> sources;
    private readonly HashSet<string> ignoredCategories;

    internal NewsSourceCatalog(IEnumerable<NewsSourceDefinition> sources)
    {
        this.sources = sources.ToDictionary(source => source.Key, StringComparer.Ordinal);
        ignoredCategories = CommonIgnoredFeedCategories
            .Select(CategoryNameNormalizer.Normalize)
            .ToHashSet(StringComparer.Ordinal);
    }

    public IReadOnlyCollection<NewsSourceDefinition> Sources => sources.Values;

    public NewsSourceDefinition? Find(string key)
    {
        return sources.GetValueOrDefault(key);
    }

    public bool IsIgnored(string normalizedCategory)
    {
        return ignoredCategories.Contains(normalizedCategory);
    }

    public static NewsSourceCatalog Load()
    {
        var assembly = typeof(NewsSourceCatalog).Assembly;
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (resourceNames.Length == 0)
        {
            throw new InvalidOperationException("The news source definitions are missing.");
        }

        var definitions = new List<NewsSourceDefinition>();

        foreach (var resourceName in resourceNames)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);

            var key = resourceName[ResourcePrefix.Length..^".json".Length];

            definitions.Add(Parse(key, buffer.ToArray()));
        }

        return new NewsSourceCatalog(definitions);
    }

    private static NewsSourceDefinition Parse(string key, byte[] content)
    {
        try
        {
            if (!KeyPattern().IsMatch(key))
            {
                throw new InvalidOperationException("The file name must be lowercase letters, digits or hyphens.");
            }

            var file = JsonSerializer.Deserialize<SourceFile>(content, JsonOptions)
                ?? throw new InvalidOperationException("The definition is empty.");

            if (file.Enabled && file.Feeds.Length == 0)
            {
                throw new InvalidOperationException("An enabled source needs at least one feed.");
            }

            string?[] urls = [file.BaseUrl, file.IconUrl, .. file.Feeds];

            foreach (var url in urls.OfType<string>())
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
                {
                    throw new InvalidOperationException($"Invalid URL: {url}.");
                }
            }

            return new NewsSourceDefinition
            {
                Key = key,
                Name = file.Name,
                BaseUrl = file.BaseUrl,
                IconUrl = file.IconUrl,
                Enabled = file.Enabled,
                Feeds = file.Feeds.Distinct(StringComparer.Ordinal).ToArray(),
                CategoryRules = new CategoryRules(file.Categories.Default, file.Categories.Ignore, file.Categories.Rules)
            };
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            throw new InvalidOperationException($"Invalid news source definition '{key}': {exception.Message}", exception);
        }
    }

    [GeneratedRegex("^[a-z0-9-]+$")]
    private static partial Regex KeyPattern();

    private sealed class SourceFile
    {
        public required string Name { get; init; }

        public required string BaseUrl { get; init; }

        public string? IconUrl { get; init; }

        public bool Enabled { get; init; } = true;

        public string[] Feeds { get; init; } = [];

        public CategoriesFile Categories { get; init; } = new();
    }

    private sealed class CategoriesFile
    {
        public string? Default { get; init; }

        public string[] Ignore { get; init; } = [];

        public Dictionary<string, CategoryRule> Rules { get; init; } = new(StringComparer.Ordinal);
    }
}
