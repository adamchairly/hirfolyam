namespace Hirfolyam.Application.Features;

public static class UrlNormalizer
{
    private static readonly HashSet<string> TrackingParameters = new(StringComparer.OrdinalIgnoreCase)
    {
        "fbclid",
        "gclid",
        "dclid",
        "msclkid",
        "mc_cid",
        "mc_eid"
    };

    public static string Normalize(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            || !string.IsNullOrEmpty(uri.UserInfo))
        {
            throw new ArgumentException("The URL must use HTTP or HTTPS without credentials.", nameof(url));
        }

        var queryParts = uri.Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Where(part => !IsTrackingParameter(part))
            .Order(StringComparer.Ordinal);

        var builder = new UriBuilder(uri)
        {
            Fragment = string.Empty,
            Query = string.Join('&', queryParts)
        };

        return builder.Uri.AbsoluteUri;
    }

    private static bool IsTrackingParameter(string part)
    {
        var key = Uri.UnescapeDataString(part.Split('=', 2)[0]);

        return key.StartsWith("utm_", StringComparison.OrdinalIgnoreCase)
            || TrackingParameters.Contains(key);
    }
}
