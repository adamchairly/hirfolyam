using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Hirfolyam.Application.Features;

public static partial class ArticleSearchText
{
    public static string FromHtml(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var document = new HtmlDocument();
        document.LoadHtml(value);

        var excludedNodes = document.DocumentNode
            .Descendants()
            .Where(node => node.Name is "script" or "style" or "iframe" or "object")
            .ToList();

        foreach (var node in excludedNodes)
        {
            node.Remove();
        }

        var blockNodes = document.DocumentNode
            .Descendants()
            .Where(node => node.Name is "p" or "div" or "br" or "li")
            .ToList();

        foreach (var node in blockNodes)
        {
            node.ParentNode?.InsertAfter(document.CreateTextNode(" "), node);
        }

        var text = WebUtility.HtmlDecode(document.DocumentNode.InnerText);

        return Whitespace().Replace(text, " ").Trim();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
