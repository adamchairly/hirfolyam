using Hirfolyam.Application.Features;
using Hirfolyam.Domain.Catalogs;

namespace Hirfolyam.Tests.Application;

public sealed class CategoryMapperTests
{
    private static readonly NewsSourceCatalog Catalog = NewsSourceCatalog.Load();

    private readonly CategoryMapper mapper = new(Catalog);

    [TestCase("femina", "Életmód", "/egeszseg/hir")]
    [TestCase("telex", "Gasztro", "/gasztro/hir")]
    [TestCase("elle", "Divat", "/divat/hir")]
    [TestCase("blikkruzs", "Otthoni Praktikák", "/otthoni-praktikak/hir")]
    [TestCase("origo", "Utazás", "/utazas/hir")]
    [TestCase("glamour", "GLAMOUR Power", "/glamour-power/hir")]
    public void LifestyleSectionsMapFromBothFeedNamesAndUrls(string key, string category, string path)
    {
        Assert.That(mapper.Map(key, [category]), Is.EqualTo(CategoryId("lifestyle")));
        Assert.That(mapper.Map(key, [], SourceUrl(key, path)), Is.EqualTo(CategoryId("lifestyle")));
    }

    [TestCase("femina", "Hazai sztár", "/hazai_sztar/hir")]
    [TestCase("elle", "Sztárhírek", "/sztarhirek/hir")]
    [TestCase("glamour", "Sztárok", "/sztarok/hir")]
    [TestCase("blikkruzs", "Horoszkóp", "/horoszkop/hir")]
    public void CelebrityAndHoroscopeSectionsRemainTabloid(string key, string category, string path)
    {
        Assert.That(mapper.Map(key, [category]), Is.EqualTo(CategoryId("tabloid")));
        Assert.That(mapper.Map(key, [], SourceUrl(key, path)), Is.EqualTo(CategoryId("tabloid")));
    }

    [TestCase("index", "Tech-tudomány", "science-tech")]
    [TestCase("hvg", "  ÍTTHON  ", "domestic")]
    [TestCase("24hu", "Nagyvilág", "world")]
    [TestCase("telex", "Techtud", "science-tech")]
    [TestCase("444", "politika", "politics")]
    [TestCase("kontroll", "Gazdaság / Tech", "economy")]
    [TestCase("ripost", "Mani", "economy")]
    [TestCase("economx", "Befektetés", "economy")]
    [TestCase("euronews", "Kulturális hírek", "culture")]
    [TestCase("portfolio", "Globál", "world")]
    [TestCase("origo", "Techbázis", "science-tech")]
    [TestCase("blikk", "Világpolitika", "world")]
    [TestCase("magyarhirlap", "Oroszország-Ukrajna", "world")]
    [TestCase("infostart", "Bűnügyek", "domestic")]
    [TestCase("nemzetisport", "Magyar válogatott", "sport")]
    [TestCase("kisalfold", "Helyi sport", "sport")]
    [TestCase("nepszava", "Bűnügy-baleset", "domestic")]
    [TestCase("demokrata", "Magyarország", "domestic")]
    [TestCase("magyarnemzet", "Autó-Motor", "science-tech")]
    [TestCase("mfor", "Makró", "economy")]
    [TestCase("privatbankar", "Részvény / Deviza / Áru", "economy")]
    [TestCase("atv", "Itthon", "domestic")]
    [TestCase("femina", "&lt;![CDATA[ Világsztár ]]&gt;", "tabloid")]
    [TestCase("borsonline", "Foci", "sport")]
    [TestCase("blikkruzs", "Otthoni Praktikák", "lifestyle")]
    [TestCase("vg", "Sportgazdaság", "economy")]
    [TestCase("liner", "Orvostudomány", "science-tech")]
    [TestCase("forbes", "Üzlet", "economy")]
    [TestCase("nyugat", "Sokkoló", "domestic")]
    [TestCase("divany", "<![CDATA[ Mindennapi ]]>", "domestic")]
    [TestCase("elle", "Decor", "lifestyle")]
    [TestCase("totalcar", "Technika", "science-tech")]
    [TestCase("mindmegette", "Alapételek", "lifestyle")]
    [TestCase("glamour", "Olvasás", "culture")]
    [TestCase("qubit", "TECH", "science-tech")]
    [TestCase("velvet", "Check-out", "lifestyle")]
    [TestCase("metropol", "VIP", "tabloid")]
    [TestCase("nlc", "Szépség &amp; Divat", "lifestyle")]
    [TestCase("media1", "Online média", "economy")]
    [TestCase("magyarnarancs", "Belpol", "politics")]
    [TestCase("atlatszo", "Világtérkép", "world")]
    [TestCase("direkt36", "Kórházi fertőzések", "domestic")]
    [TestCase("valaszonline", "diplomácia", "world")]
    [TestCase("police", "Határrendészet", "domestic")]
    [TestCase("penzcentrum", "Szórakozás", "tabloid")]
    [TestCase("penzcentrum", "Nyugdíj", "economy")]
    [TestCase("nationalgeographic", "Természet", "science-tech")]
    [TestCase("agrarszektor", "Növény", "economy")]
    [TestCase("magyarmezogazdasag", "Természet", "science-tech")]
    [TestCase("csakfoci", "Külföldi körkép", "sport")]
    [TestCase("vezess", "Forma-1", "sport")]
    [TestCase("egeszsegkalauz", "Orvostudományi kutatások", "science-tech")]
    [TestCase("maszol", "Belföld", "world")]
    public void MapsKnownSourceCategories(string key, string category, string expectedSlug)
    {
        Assert.That(mapper.Map(key, [category]), Is.EqualTo(CategoryId(expectedSlug)));
    }

    [TestCase("hvg", "/itthon/20261007_iskola", "domestic")]
    [TestCase("magyarhirlap", "/montazs/20261007-repulogep", "tabloid")]
    [TestCase("magyarhirlap", "/oroszorszag-ukrajna/20261007-legicsapasok", "world")]
    [TestCase("nemzetisport", "/magyar-valogatott/2026/10/futsal", "sport")]
    [TestCase("kontroll", "/cikk/gazdasag-tech/2026/10/hir", "economy")]
    [TestCase("origo", "/sport/f1/2026/10/futam", "sport")]
    [TestCase("blikk", "/politika/vilagpolitika/hir", "world")]
    [TestCase("blikk", "/sztarvilag/kultura/hir", "culture")]
    [TestCase("euronews", "/culture/food-and-drink/hir", "lifestyle")]
    [TestCase("privatbankar", "/cikkek/nemzetkozi/hir.html", "world")]
    [TestCase("femina", "/hazai_sztar/hir/", "tabloid")]
    [TestCase("kisalfold", "/helyi-sport/2026/10/hir", "sport")]
    [TestCase("hirtv", "/ahirtvhirei/2026/10/hir", "domestic")]
    [TestCase("hirtv", "/hirtvkulfold/2026/10/hir", "world")]
    [TestCase("mta", "/mta_hirei/hir-115732", "science-tech")]
    [TestCase("vezess", "/vezetunk/2026/10/hir", "science-tech")]
    [TestCase("noklapja", "/gasztro/2026/10/10/hir", "lifestyle")]
    [TestCase("maszol", "/belfold/hir", "world")]
    public void UrlSectionTakesPriorityOverUnrelatedTags(string key, string path, string expectedSlug)
    {
        var result = mapper.Map(key, ["politika", "tudomány"], SourceUrl(key, path));

        Assert.That(result, Is.EqualTo(CategoryId(expectedSlug)));
    }

    [TestCase("https://www.hvg.hu/ITTHON/2026/hir")]
    [TestCase("https://hvg.hu/itthon/hir?next=/sport/")]
    [TestCase("https://hvg.hu/%69tthon/hir")]
    public void MatchesUrlSegmentsWithHostAndEncodingNormalization(string url)
    {
        Assert.That(mapper.Map("hvg", [], url), Is.EqualTo(CategoryId("domestic")));
    }

    [TestCase("https://hvg.hu/itthonrol/hir")]
    [TestCase("https://hvg.hu/unknown/itthon/hir")]
    [TestCase("https://hvg.hu/2026/10/sport-gazdasag")]
    [TestCase("https://hvg.hu/hir?section=/itthon/")]
    [TestCase("https://other.example/itthon/hir")]
    [TestCase("https://hvg.hu.evil.invalid/itthon/hir")]
    [TestCase("file:///itthon/hir")]
    [TestCase("not a URL")]
    public void DoesNotGuessFromArticleSlugsOrOtherHosts(string url)
    {
        Assert.That(mapper.Map("hvg", [], url), Is.Null);
    }

    [TestCase("euronews", "Videó")]
    [TestCase("nyugat", "Podcast")]
    [TestCase("telex", "English")]
    [TestCase("kisalfold", "Ország-világ")]
    [TestCase("hvg", "hvg360")]
    [TestCase("valaszonline", "Interjú")]
    public void MixedSectionsDoNotForceACategory(string key, string category)
    {
        Assert.That(mapper.Map(key, [category], "https://example.com/2026/10/hir"), Is.Null);
    }

    [TestCase("nemzetisport", "sport")]
    [TestCase("mindmegette", "lifestyle")]
    [TestCase("police", "domestic")]
    [TestCase("mta", "science-tech")]
    [TestCase("nationalgeographic", "science-tech")]
    [TestCase("bankmonitor", "economy")]
    [TestCase("agrarszektor", "economy")]
    [TestCase("magyarmezogazdasag", "economy")]
    [TestCase("csakfoci", "sport")]
    [TestCase("vezess", "science-tech")]
    [TestCase("noklapja", "lifestyle")]
    [TestCase("egeszsegkalauz", "lifestyle")]
    public void UsesAnExplicitSourceDefaultWhenTheFeedHasNoCategory(string key, string expectedSlug)
    {
        Assert.That(mapper.Map(key, []), Is.EqualTo(CategoryId(expectedSlug)));
    }

    [Test]
    public void DoesNotApplyOneSourcesAliasToAnotherSource()
    {
        Assert.That(mapper.Map("hvg", ["Globál"]), Is.Null);
    }

    [Test]
    public void FeedCategoriesThatNameACategoryWorkForEverySource()
    {
        Assert.That(mapper.Map("unknown-source", ["  KÜLFÖLD  "]), Is.EqualTo(CategoryId("world")));
        Assert.That(mapper.Map("unknown-source", ["unknown", "SCIENCE-TECH"]), Is.EqualTo(CategoryId("science-tech")));
        Assert.That(mapper.Map("unknown-source", ["tech"]), Is.Null);
    }

    [Test]
    public void CommonlyIgnoredFeedCategoriesNeverAssignACategory()
    {
        Assert.That(mapper.Map("unknown-source", ["Videó", "Podcast"]), Is.Null);
    }

    private static string SourceUrl(string key, string path)
    {
        return Catalog.Find(key)!.BaseUrl.TrimEnd('/') + path;
    }

    private static int CategoryId(string slug)
    {
        return Categories.All.Single(category => category.Slug == slug).Id;
    }
}
