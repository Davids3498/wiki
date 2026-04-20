using System.Text.Json.Serialization;
using Allure.NUnit.Attributes;
using Microsoft.Playwright;

namespace WikipediaAutomation.Pages;

public sealed class WikipediaArticlePage : BasePage
{
    private const string DebuggingFeaturesHeadingId = "Debugging_features";
    private const string MicrosoftDevToolsNavboxAriaLabel = "Microsoft development tools";

    public AppearancePanelComponent Appearance { get; }

    private ILocator DebuggingFeaturesHeading => Page.Locator($"#{DebuggingFeaturesHeadingId}");

    private ILocator DebuggingFeaturesSection =>
        Page.Locator($"div.mw-heading:has(#{DebuggingFeaturesHeadingId})");

    private ILocator MicrosoftDevToolsNavbox =>
        Page.GetByRole(AriaRole.Navigation, new() { Name = MicrosoftDevToolsNavboxAriaLabel }).First;

    public WikipediaArticlePage(IPage page) : base(page)
    {
        Appearance = new AppearancePanelComponent(page);
    }

    [AllureStep("Open Wikipedia article at {url}")]
    public async Task GotoAsync(string url)
    {
        await Page.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
        });
        await DismissCookieBannerIfPresentAsync();
        // Stability anchor: wait for the main content container rather than a fixed sleep.
        await Page.Locator("#mw-content-text").First.WaitForAsync();
    }

    /// <summary>
    /// Wikipedia occasionally serves a CentralNotice or cookie consent banner.
    /// Tries a few common dismiss buttons; silent if none are present.
    /// </summary>
    [AllureStep("Dismiss cookie/consent banner if present")]
    public async Task DismissCookieBannerIfPresentAsync()
    {
        // CentralNotice close
        var centralNoticeClose = Page.Locator("#centralNotice .cn-closeButton").First;
        if (await centralNoticeClose.IsVisibleAsync())
        {
            await centralNoticeClose.ClickAsync();
        }

        // Generic cookie consent banner (WMF has historically used various UIs)
        foreach (var text in new[] { "Accept", "I agree", "OK", "Got it", "Accept all" })
        {
            var btn = Page.GetByRole(AriaRole.Button, new() { Name = text }).First;
            if (await btn.IsVisibleAsync())
            {
                await btn.ClickAsync();
                break;
            }
        }
    }

    [AllureStep("Extract \"Debugging features\" section text via UI")]
    public async Task<string> GetDebuggingFeaturesSectionTextAsync()
    {
        await DebuggingFeaturesHeading.WaitForAsync();

        // CSS has no "siblings until next heading" selector; XPath following-sibling with
        // a preceding-sibling[1] guard gives us exactly the elements in this section.
        var xpath =
            $"xpath=//div[contains(@class,'mw-heading')][descendant::*[@id='{DebuggingFeaturesHeadingId}']]" +
            "/following-sibling::*" +
            "[not(contains(@class,'mw-heading')) " +
            "and preceding-sibling::div[contains(@class,'mw-heading')][1]" +
            $"[descendant::*[@id='{DebuggingFeaturesHeadingId}']]]";

        var parts = await Page.Locator(xpath).AllInnerTextsAsync();
        return string.Join("\n", parts);
    }

    [AllureStep("Scroll to Debugging features heading")]
    public async Task ScrollToDebuggingFeaturesAsync()
    {
        await DebuggingFeaturesSection.ScrollIntoViewIfNeededAsync();
    }

    [AllureStep("Scroll to Microsoft development tools navbox")]
    public async Task<ILocator> GotoMicrosoftDevelopmentToolsAsync()
    {
        await MicrosoftDevToolsNavbox.ScrollIntoViewIfNeededAsync();
        return MicrosoftDevToolsNavbox;
    }

    [AllureStep("Collect technology entries from the Microsoft development tools navbox")]
    public async Task<IReadOnlyList<TechnologyEntry>> GetMicrosoftDevelopmentToolsEntriesAsync()
    {
        var navbox = await GotoMicrosoftDevelopmentToolsAsync();
        await navbox.WaitForAsync();

        // Single EvaluateAsync round-trip; childNodes walk distinguishes own label text from nested list text.
        var raw = await navbox.EvaluateAsync<TechnologyEntry[]>(
            @"root => {
                const results = [];
                const lis = root.querySelectorAll('td.navbox-list li, td.navbox-list-with-group li');
                lis.forEach(li => {
                    // Skip navbar 'v/t/e' helper links.
                    if (li.closest('.navbar')) return;
                    // Skip any <li> that contains a nested list - those are group headers,
                    // and their leaf children will be visited in their own right.
                    if (li.querySelector(':scope > ul, :scope > ol')) return;
                    let own = '';
                    let firstElementChildOfOwnText = null;
                    for (const child of li.childNodes) {
                        if (child.nodeType === Node.TEXT_NODE) {
                            own += child.textContent;
                        } else if (child.nodeType === Node.ELEMENT_NODE) {
                            if (child.tagName === 'UL' || child.tagName === 'OL') continue;
                            own += child.innerText || child.textContent || '';
                            firstElementChildOfOwnText ??= child;
                        }
                    }
                    const name = (own || '').trim();
                    if (!name) return;
                    const isLink = firstElementChildOfOwnText !== null
                        && firstElementChildOfOwnText.tagName === 'A';
                    results.push({ name, isLink });
                });
                return results;
            }");

        return raw;
    }
}

public sealed class TechnologyEntry
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("isLink")]
    public bool IsLink { get; init; }
}
