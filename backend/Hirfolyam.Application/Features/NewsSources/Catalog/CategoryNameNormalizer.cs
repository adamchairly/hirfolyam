using System.Globalization;
using System.Net;
using System.Text;

namespace Hirfolyam.Application.Features;

internal static class CategoryNameNormalizer
{
    public static string Normalize(string value)
    {
        var decoded = WebUtility.HtmlDecode(value).Trim();

        // Some feeds encode CDATA markers as category text
        if (decoded.StartsWith("<![CDATA[", StringComparison.Ordinal)
            && decoded.EndsWith("]]>", StringComparison.Ordinal))
        {
            decoded = decoded[9..^3].Trim();
        }

        var decomposed = decoded.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var result = new StringBuilder();

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                result.Append(character);
            }
        }

        return result.ToString().Normalize(NormalizationForm.FormC);
    }
}
