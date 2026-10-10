using Hirfolyam.Application.Features;

namespace Hirfolyam.Tests.Application;

public sealed class NewsSourceCatalogTests
{
    [Test]
    public void UnknownCategoriesInRulesAreRejected()
    {
        var rules = new Dictionary<string, CategoryRule>
        {
            ["gossip"] = new() { Feed = ["Pletyka"] }
        };

        Assert.That(() => new CategoryRules(null, [], rules), Throws.InvalidOperationException);
        Assert.That(
            () => new CategoryRules("gossip", [], new Dictionary<string, CategoryRule>()),
            Throws.InvalidOperationException);
    }

    [Test]
    public void ConflictingRulesAreRejected()
    {
        var rules = new Dictionary<string, CategoryRule>
        {
            ["sport"] = new() { Feed = ["Foci"] },
            ["culture"] = new() { Feed = ["  FOCI  "] }
        };

        Assert.That(() => new CategoryRules(null, [], rules), Throws.InvalidOperationException);
    }

    [TestCase("sport")]
    [TestCase("/sport?x=1")]
    [TestCase("/sport#top")]
    public void InvalidPathsAreRejected(string path)
    {
        var rules = new Dictionary<string, CategoryRule>
        {
            ["sport"] = new() { Paths = [path] }
        };

        Assert.That(() => new CategoryRules(null, [], rules), Throws.InvalidOperationException);
    }
}
