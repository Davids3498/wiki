using Allure.NUnit.Attributes;
using Microsoft.Playwright;

namespace WikipediaAutomation.Pages;

public sealed class AppearancePanelComponent : BasePage
{
    public const string LightThemeClass = "skin-theme-clientpref-day";
    public const string DarkThemeClass = "skin-theme-clientpref-night";
    public const string AutoThemeClass = "skin-theme-clientpref-os";

    private const string DarkOptionLabel = "Dark";

    private ILocator AppearanceDropdownToggle =>
        Page.Locator("#vector-appearance-dropdown-label");

    // The radio group exists in two DOM copies: pinned (right rail) and unpinned (dropdown).
    private ILocator PinnedDarkOption =>
        Page.Locator("#vector-appearance-pinned-container")
            .GetByLabel(DarkOptionLabel, new() { Exact = true });

    private ILocator UnpinnedDarkOption =>
        Page.Locator("#vector-appearance-unpinned-container")
            .GetByLabel(DarkOptionLabel, new() { Exact = true });

    // Used only to wait for client-side JS to inject the radio group.
    private ILocator AnyDarkOption =>
        Page.Locator("nav.vector-appearance-landmark")
            .GetByLabel(DarkOptionLabel, new() { Exact = true });

    public AppearancePanelComponent(IPage page) : base(page)
    {
    }

    [AllureStep("Select the Dark color theme from the Appearance (Color beta) panel")]
    public async Task SelectDarkThemeAsync()
    {
        await AnyDarkOption.First.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Attached,
        });

        if (await PinnedDarkOption.IsVisibleAsync())
        {
            await PinnedDarkOption.CheckAsync();
            return;
        }

        await AppearanceDropdownToggle.ClickAsync();
        await UnpinnedDarkOption.WaitForAsync();
        await UnpinnedDarkOption.CheckAsync();
    }

    public Task<string> GetHtmlClassListAsync() =>
        Page.Locator("html").First.EvaluateAsync<string>("el => el.className");

    public async Task<bool> IsDarkThemeActiveAsync()
    {
        var classes = await GetHtmlClassListAsync();
        return classes.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Contains(DarkThemeClass);
    }
}
