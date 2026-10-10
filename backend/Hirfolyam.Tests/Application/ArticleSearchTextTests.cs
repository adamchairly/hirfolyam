using Hirfolyam.Application.Features;

namespace Hirfolyam.Tests.Application;

public sealed class ArticleSearchTextTests
{
    [TestCase(null, "")]
    [TestCase("", "")]
    [TestCase("  \n\t ", "")]
    [TestCase("Árvíztűrő TÜKÖRFÚRÓGÉP", "Árvíztűrő TÜKÖRFÚRÓGÉP")]
    [TestCase("<p>Első</p><div>Második<br>Harmadik</div>", "Első Második Harmadik")]
    [TestCase("<ul><li>Egy</li><li>Kettő</li></ul>", "Egy Kettő")]
    [TestCase("<b>adó</b>változás", "adóváltozás")]
    [TestCase("Ár &amp; adó&nbsp; &#337; &#x171; &lt; 5", "Ár & adó ő ű < 5")]
    [TestCase("<a href='https://hidden.example'>Látható</a><img alt='Rejtett' src='image.jpg'>", "Látható")]
    [TestCase("<script>rejtett</script><style>rejtett</style><iframe>rejtett</iframe><object>rejtett</object>Szöveg", "Szöveg")]
    [TestCase("<!-- Rejtett -->Látható", "Látható")]
    [TestCase("<p>Befejezetlen <b>leírás", "Befejezetlen leírás")]
    public void ConvertsDescriptionToVisibleText(string? html, string expected)
    {
        var text = ArticleSearchText.FromHtml(html);

        Assert.That(text, Is.EqualTo(expected));
    }
}
