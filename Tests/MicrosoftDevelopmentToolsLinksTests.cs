using Allure.NUnit;
using Allure.NUnit.Attributes;
using Allure.Net.Commons;
using NUnit.Framework;
using WikipediaAutomation.Pages;
using WikipediaAutomation.Tests.Base;

namespace WikipediaAutomation.Tests;

[TestFixture]
[AllureNUnit]
[AllureSuite("Task 2 - Microsoft development tools")]
[AllureFeature("All technology names must be hyperlinks")]
[AllureLabel("layer", "ui")]
public sealed class MicrosoftDevelopmentToolsLinksTests : PlaywrightTestBase
{
    [Test]
    [AllureName("Every technology entry in the Microsoft development tools navbox is a hyperlink")]
    [AllureDescription(
        "Scrolls to the Microsoft development tools navbox below the Debugging features " +
        "section, enumerates every technology entry, and asserts that each one renders " +
        "as a hyperlink. Each entry is wrapped in an Allure step for per-technology visibility.")]
    [AllureTag("task2", "parameterized", "data-driven")]
    public async Task Every_technology_is_a_hyperlink()
    {
        var article = NewArticlePage();
        await article.GotoAsync(Settings.ArticleUri.ToString());

        IReadOnlyList<TechnologyEntry> entries = await article.GetMicrosoftDevelopmentToolsEntriesAsync();

        TestContext.Out.WriteLine($"Collected {entries.Count} technology entries from the navbox.");
        Assert.That(entries, Is.Not.Empty,
            "Expected at least one technology entry inside the Microsoft development tools navbox - " +
            "selector likely broken or page structure changed.");

        Assert.Multiple(() =>
        {
            foreach (var entry in entries)
            {
                AllureApi.Step(entry.Name, () =>
                {
                    Assert.That(
                        entry.IsLink,
                        Is.True,
                        $"Technology \"{entry.Name}\" is not rendered as a hyperlink.");
                });
            }
        });
    }
}