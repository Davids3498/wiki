using Allure.NUnit;
using Allure.NUnit.Attributes;
using NUnit.Framework;
using WikipediaAutomation.Pages;
using WikipediaAutomation.Tests.Base;

namespace WikipediaAutomation.Tests;

[TestFixture]
[AllureNUnit]
[AllureSuite("Task 3 - Appearance (Color beta)")]
[AllureFeature("Dark theme toggle")]
[AllureLabel("layer", "ui")]
public sealed class AppearancePanelDarkModeTests : PlaywrightTestBase
{
    [Test]
    [AllureName("Selecting Dark in the Appearance panel applies the Dark theme to <html>")]
    [AllureDescription(
        "Opens the Appearance (Color beta) panel in the Vector 2022 right sidebar, " +
        "selects the Dark theme, and asserts that the <html> element gains the " +
        "'skin-theme-clientpref-night' class that Wiki uses to activate Dark mode.")]
    [AllureTag("task3", "theme")]
    public async Task Switching_to_dark_updates_html_class()
    {
        var article = NewArticlePage();
        await article.GotoAsync(Settings.ArticleUri.ToString());

        var classesBefore = await article.Appearance.GetHtmlClassListAsync();
        TestContext.Out.WriteLine($"<html> classes before: {classesBefore}");
        Assert.That(
            classesBefore.Split(' ').Contains(AppearancePanelComponent.DarkThemeClass),
            Is.False,
            "Precondition failed: Dark theme class is already present before toggling.");

        await article.Appearance.SelectDarkThemeAsync();

        var classesAfter = await article.Appearance.GetHtmlClassListAsync();
        TestContext.Out.WriteLine($"<html> classes after : {classesAfter}");

        Assert.That(
            await article.Appearance.IsDarkThemeActiveAsync(),
            Is.True,
            () => $"Expected <html> to carry '{AppearancePanelComponent.DarkThemeClass}' " +
                  $"after selecting Dark, but classes are: {classesAfter}");
    }
}
