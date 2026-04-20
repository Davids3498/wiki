using NUnit.Framework;
using WikipediaAutomation.Helpers;

namespace WikipediaAutomation.Tests.Unit;

[TestFixture]
public sealed class HtmlStripperTests
{
    [Test]
    public void ToPlainText_returns_empty_for_empty_input()
    {
        Assert.That(HtmlStripper.ToPlainText(string.Empty), Is.Empty);
    }

    [Test]
    public void ToPlainText_returns_empty_for_whitespace_only_input()
    {
        Assert.That(HtmlStripper.ToPlainText("   \t\n"), Is.Empty);
    }

    [Test]
    public void ToPlainText_strips_tags_and_preserves_inner_text()
    {
        var html = "<p>Hello <strong>world</strong></p>";
        var result = HtmlStripper.ToPlainText(html);
        Assert.That(result, Does.Contain("Hello"));
        Assert.That(result, Does.Contain("world"));
        Assert.That(result, Does.Not.Contain("<"));
        Assert.That(result, Does.Not.Contain(">"));
    }

    [Test]
    public void ToPlainText_removes_script_block_contents()
    {
        var html = "<p>visible</p><script>alert('x');</script>";
        var result = HtmlStripper.ToPlainText(html);
        Assert.That(result, Does.Contain("visible"));
        Assert.That(result, Does.Not.Contain("alert"));
    }

    [Test]
    public void ToPlainText_removes_style_block_contents()
    {
        var html = "<style>body { color: red; }</style><p>hi</p>";
        var result = HtmlStripper.ToPlainText(html);
        Assert.That(result, Does.Contain("hi"));
        Assert.That(result, Does.Not.Contain("color"));
    }

    [Test]
    public void ToPlainText_removes_mediawiki_references_list()
    {
        // The Parse API auto-appends a references list for prop=text single-section
        // responses. The UI view doesn't render it inline, so stripping it keeps the
        // UI/API word-count comparison fair.
        var html = "<p>body text</p><ol class=\"references\"><li>Ref 1</li><li>Ref 2</li></ol>";
        var result = HtmlStripper.ToPlainText(html);
        Assert.That(result, Does.Contain("body text"));
        Assert.That(result, Does.Not.Contain("Ref 1"));
        Assert.That(result, Does.Not.Contain("Ref 2"));
    }

    [Test]
    public void ToPlainText_removes_mw_heading_wrapper_so_section_title_does_not_count()
    {
        var html = "<div class=\"mw-heading mw-heading3\"><h3>Debugging features</h3></div><p>body</p>";
        var result = HtmlStripper.ToPlainText(html);
        Assert.That(result, Does.Contain("body"));
        Assert.That(result, Does.Not.Contain("Debugging"));
    }

    [Test]
    public void ToPlainText_decodes_html_entities()
    {
        var result = HtmlStripper.ToPlainText("Tom &amp; Jerry &#8211; friends");
        Assert.That(result, Does.Contain("Tom & Jerry"));
        Assert.That(result, Does.Not.Contain("&amp;"));
    }
}
