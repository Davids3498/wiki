using System.Net;
using System.Text.RegularExpressions;

namespace WikipediaAutomation.Helpers;

public static partial class HtmlStripper
{
    [GeneratedRegex(@"<script\b[^>]*>.*?</script>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex ScriptRegex();

    [GeneratedRegex(@"<style\b[^>]*>.*?</style>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex StyleRegex();

    [GeneratedRegex(@"<div class=""mw-heading[^""]*"">.*?</div>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex HeadingBlockRegex();

    [GeneratedRegex(@"<span class=""mw-editsection"">.*?</span></span>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex EditSectionRegex();

    // prop=text appends an inline references list not shown in the rendered UI; strip it to avoid inflating word counts.
    [GeneratedRegex(@"<ol class=""references[^""]*"">.*?</ol>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex ReferencesListRegex();

    [GeneratedRegex(@"<[^>]+>", RegexOptions.Compiled)]
    private static partial Regex AnyTagRegex();

    public static string ToPlainText(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var working = ScriptRegex().Replace(html, " ");
        working = StyleRegex().Replace(working, " ");
        working = ReferencesListRegex().Replace(working, " ");
        working = HeadingBlockRegex().Replace(working, " ");
        working = EditSectionRegex().Replace(working, " ");
        working = AnyTagRegex().Replace(working, " ");
        return WebUtility.HtmlDecode(working);
    }
}
