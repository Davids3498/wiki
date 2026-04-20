using Allure.NUnit;
using Allure.NUnit.Attributes;
using NUnit.Framework;
using WikipediaAutomation.Helpers;
using WikipediaAutomation.Helpers.Api;
using WikipediaAutomation.Tests.Base;
using static WikipediaAutomation.Tests.Support.TestAttachments;

namespace WikipediaAutomation.Tests;

[TestFixture]
[AllureNUnit]
[AllureSuite("Task 1 - UI vs API parity")]
[AllureFeature("Debugging features section")]
[AllureLabel("layer", "ui-api")]
public sealed class DebuggingFeaturesParityTests : PlaywrightTestBase
{
    private const string PageTitle = "Playwright_(software)";
    private const string SectionName = "Debugging features";

    [Test]
    [AllureName("UI and API produce the same unique-word count for the Debugging features section")]
    [AllureDescription(
        "Extracts the 'Debugging features' section text via both the rendered UI (POM) " +
        "and the Wikipedia Parse API. Normalizes both (lowercase, strip citations and " +
        "punctuation, collapse whitespace), counts unique words, and asserts parity.")]
    [AllureTag("task1", "parity")]
    public async Task Unique_word_count_matches_between_ui_and_api()
    {
        // 1. Pull the section text via UI (POM).
        var article = NewArticlePage();
        await article.GotoAsync(Settings.ArticleUri.ToString());
        await article.ScrollToDebuggingFeaturesAsync();
        var uiText = await article.GetDebuggingFeaturesSectionTextAsync();
        AttachText("ui-raw.txt", uiText);

        // 2. Pull the same section via the Wiki Parse API and convert to plain text.
        using var client = new WikipediaApiClient(Settings);
        var apiHtml = await client.GetSectionHtmlByTitleAsync(PageTitle, SectionName);
        var apiText = HtmlStripper.ToPlainText(apiHtml);
        AttachText("api-raw.txt", apiText);

        // 3. Normalize + compute unique word sets.
        var uiWords = TextNormalizer.UniqueWords(uiText);
        var apiWords = TextNormalizer.UniqueWords(apiText);
        AttachText("ui-normalized.txt", TextNormalizer.Normalize(uiText));
        AttachText("api-normalized.txt", TextNormalizer.Normalize(apiText));

        var onlyInUi = uiWords.Except(apiWords).OrderBy(s => s, StringComparer.Ordinal).ToList();
        var onlyInApi = apiWords.Except(uiWords).OrderBy(s => s, StringComparer.Ordinal).ToList();

        TestContext.Out.WriteLine($"UI unique words : {uiWords.Count}");
        TestContext.Out.WriteLine($"API unique words: {apiWords.Count}");
        if (onlyInUi.Count > 0 || onlyInApi.Count > 0)
        {
            TestContext.Out.WriteLine($"Only in UI : [{string.Join(", ", onlyInUi)}]");
            TestContext.Out.WriteLine($"Only in API: [{string.Join(", ", onlyInApi)}]");
        }

        // 4. Assert parity.
        Assert.That(uiWords, Is.Not.Empty, "UI extraction yielded no words - selectors likely broken.");
        Assert.That(apiWords, Is.Not.Empty, "API extraction yielded no words - API response likely malformed.");
        Assert.That(
            uiWords.Count,
            Is.EqualTo(apiWords.Count),
            () => $"Unique word count mismatch. UI={uiWords.Count}, API={apiWords.Count}. " +
                  $"Only in UI: [{string.Join(", ", onlyInUi)}]. " +
                  $"Only in API: [{string.Join(", ", onlyInApi)}].");
    }
}
