using Microsoft.Playwright;
using NUnit.Framework;
using WikipediaAutomation.Config;
using WikipediaAutomation.Pages;

namespace WikipediaAutomation.Tests.Base;

public abstract class PlaywrightTestBase
{
    protected TestSettings Settings { get; private set; } = null!;
    private IPlaywright _playwright = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetupAsync()
    {
        Settings = TestSettings.Current;
        _playwright = await Playwright.CreateAsync();

        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = ResolveHeadless(Settings.Browser.Headless),
            SlowMo = Settings.Browser.SlowMoMs,
        };

        Browser = Settings.Browser.Name.ToLowerInvariant() switch
        {
            "chromium" => await _playwright.Chromium.LaunchAsync(launchOptions),
            "firefox" => await _playwright.Firefox.LaunchAsync(launchOptions),
            "webkit" => await _playwright.Webkit.LaunchAsync(launchOptions),
            var other => throw new NotSupportedException($"Browser '{other}' is not supported."),
        };
    }

    [SetUp]
    public async Task SetUpAsync()
    {
        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = Settings.Browser.ViewportWidth,
                Height = Settings.Browser.ViewportHeight,
            },
            Locale = "en-US",
            UserAgent = Settings.Api.UserAgent, // explicit UA reduces A/B banner noise
        });

        Context.SetDefaultTimeout(Settings.Browser.DefaultTimeoutMs);
        Context.SetDefaultNavigationTimeout(Settings.Browser.NavigationTimeoutMs);

        if (Settings.Tracing.Enabled)
        {
            await Context.Tracing.StartAsync(new TracingStartOptions
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true,
                Title = TestContext.CurrentContext.Test.FullName,
            });
        }

        Page = await Context.NewPageAsync();
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        var result = TestContext.CurrentContext.Result;
        var testFailed = result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed;

        try
        {
            if (Settings.Tracing.Enabled)
            {
                var outputDir = Path.Combine(AppContext.BaseDirectory, Settings.Tracing.OutputDirectory);
                Directory.CreateDirectory(outputDir);
                var safeName = MakePathSafe(TestContext.CurrentContext.Test.FullName);
                var tracePath = Path.Combine(outputDir, $"{safeName}.zip");
                await Context.Tracing.StopAsync(new TracingStopOptions { Path = tracePath });
                if (testFailed)
                {
                    TestContext.AddTestAttachment(tracePath, "Playwright trace");
                }
            }

            if (testFailed)
            {
                var screenshotDir = Path.Combine(AppContext.BaseDirectory, "test-results", "screenshots");
                Directory.CreateDirectory(screenshotDir);
                var screenshotPath = Path.Combine(
                    screenshotDir,
                    $"{MakePathSafe(TestContext.CurrentContext.Test.FullName)}.png");
                await Page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = screenshotPath,
                    FullPage = true,
                });
                TestContext.AddTestAttachment(screenshotPath, "Failure screenshot");
            }
        }
        finally
        {
            await Page.CloseAsync();
            await Context.CloseAsync();
        }
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await Browser.CloseAsync();
        _playwright.Dispose();
    }

    protected WikipediaArticlePage NewArticlePage() => new(Page);

    private static bool ResolveHeadless(bool fallback)
    {
        var fromRunSettings = TestContext.Parameters["Headless"];
        if (!string.IsNullOrWhiteSpace(fromRunSettings)
            && bool.TryParse(fromRunSettings, out var value))
        {
            return value;
        }

        var fromEnv = Environment.GetEnvironmentVariable("WA_HEADLESS");
        if (!string.IsNullOrWhiteSpace(fromEnv) && bool.TryParse(fromEnv, out var envValue))
        {
            return envValue;
        }

        return fallback;
    }

    private static string MakePathSafe(string name)
    {
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(invalid, '_');
        }
        return name;
    }
}
