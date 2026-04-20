using NUnit.Framework;
using WikipediaAutomation.Helpers;

namespace WikipediaAutomation.Tests.Unit;

[TestFixture]
public sealed class TextNormalizerTests
{
    [Test]
    public void Normalize_returns_empty_for_null()
    {
        Assert.That(TextNormalizer.Normalize(null), Is.Empty);
    }

    [Test]
    public void Normalize_returns_empty_for_whitespace_only_input()
    {
        Assert.That(TextNormalizer.Normalize("   \t\n"), Is.Empty);
    }

    [Test]
    public void Normalize_lowercases_ascii()
    {
        Assert.That(TextNormalizer.Normalize("Hello WORLD"), Is.EqualTo("hello world"));
    }

    [TestCase("foo[1] bar", "foo bar")]
    [TestCase("foo[15] bar[234]", "foo bar")]
    [TestCase("alpha[note 3] beta", "alpha beta")]
    public void Normalize_strips_wikipedia_citation_markers(string input, string expected)
    {
        Assert.That(TextNormalizer.Normalize(input), Is.EqualTo(expected));
    }

    [Test]
    public void Normalize_replaces_punctuation_with_space_so_words_do_not_merge()
    {
        // If punctuation were just deleted, "foo,bar" would become "foobar" and collapse
        // two distinct words into one. The normalizer replaces punctuation with spaces.
        Assert.That(TextNormalizer.Normalize("foo,bar;baz"), Is.EqualTo("foo bar baz"));
    }

    [Test]
    public void Normalize_collapses_nbsp_and_mixed_whitespace_to_single_spaces()
    {
        // \u00A0 is a non-breaking space - Wikipedia uses them liberally.
        Assert.That(TextNormalizer.Normalize("foo\u00A0\u00A0bar  \tbaz"), Is.EqualTo("foo bar baz"));
    }

    [Test]
    public void UniqueWords_deduplicates_case_insensitively()
    {
        var set = TextNormalizer.UniqueWords("foo bar Foo BAR foo");
        Assert.That(set, Is.EquivalentTo(new[] { "foo", "bar" }));
    }

    [Test]
    public void UniqueWords_returns_empty_set_for_null_input()
    {
        Assert.That(TextNormalizer.UniqueWords(null), Is.Empty);
    }

    [Test]
    public void UniqueWords_returns_empty_set_for_whitespace_only_input()
    {
        Assert.That(TextNormalizer.UniqueWords("   \t\n"), Is.Empty);
    }

    [Test]
    public void UniqueWords_treats_citation_markers_and_punctuation_as_noise()
    {
        var set = TextNormalizer.UniqueWords("Playwright[15] is, a framework. Playwright rocks!");
        Assert.That(set, Is.EquivalentTo(new[] { "playwright", "is", "a", "framework", "rocks" }));
    }
}
