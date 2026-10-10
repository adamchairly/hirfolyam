using Hirfolyam.Application.Features;

namespace Hirfolyam.Tests.Application;

public sealed class UrlNormalizerTests
{
    [Test]
    public void NormalizeRemovesTrackingAndPreservesArticleParameters()
    {
        var normalized = UrlNormalizer.Normalize("HTTPS://EXAMPLE.COM:443/article?id=23&utm_source=rss&b=2&fbclid=123&a=1#comments");

        Assert.That(normalized, Is.EqualTo("https://example.com/article?a=1&b=2&id=23"));
        Assert.That(UrlNormalizer.Normalize("https://example.com/article?id=24&b=2&a=1"), Is.Not.EqualTo(normalized));
    }

    [TestCase("file:///etc/passwd")]
    [TestCase("javascript:alert(1)")]
    [TestCase("https://user:password@example.com/article")]
    public void NormalizeRejectsInvalidUrls(string url)
    {
        Assert.Throws<ArgumentException>(() => UrlNormalizer.Normalize(url));
    }
}
