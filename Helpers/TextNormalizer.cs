using System.Globalization;
using System.Text.RegularExpressions;

namespace WikipediaAutomation.Helpers;

public static partial class TextNormalizer
{
    [GeneratedRegex(@"\[[A-Za-z0-9\s]+\]", RegexOptions.Compiled)]
    private static partial Regex CitationMarkerRegex();

    // \s+ covers NBSP (\u00A0) so it collapses correctly.
    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();

    public static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var withoutCitations = CitationMarkerRegex().Replace(raw, " ");
        var lowered = withoutCitations.ToLower(CultureInfo.InvariantCulture);
        var withoutPunctuation = StripPunctuation(lowered);
        var collapsed = WhitespaceRegex().Replace(withoutPunctuation, " ").Trim();
        return collapsed;
    }

    public static IReadOnlySet<string> UniqueWords(string? raw)
    {
        var normalized = Normalize(raw);
        if (normalized.Length == 0)
        {
            return new HashSet<string>();
        }

        return normalized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();
    }

    private static string StripPunctuation(string input)
    {
        var buffer = new System.Text.StringBuilder(input.Length);
        foreach (var ch in input)
        {
            if (char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch))
            {
                buffer.Append(ch);
            }
            else
            {
                buffer.Append(' '); // space instead of removal so adjacent words don't merge
            }
        }

        return buffer.ToString();
    }
}
