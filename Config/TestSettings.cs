using Microsoft.Extensions.Configuration;

namespace WikipediaAutomation.Config;

public sealed class TestSettings
{
    public required string BaseUrl { get; init; }
    public required string ArticlePath { get; init; }
    public required string ApiPath { get; init; }
    public required BrowserSettings Browser { get; init; }
    public required ApiSettings Api { get; init; }
    public TracingSettings Tracing { get; init; } = new();

    public Uri ArticleUri => new(new Uri(BaseUrl), ArticlePath);
    public Uri ApiBaseUri => new(new Uri(BaseUrl), ApiPath);

    private static readonly Lazy<TestSettings> _current = new(Load);
    public static TestSettings Current => _current.Value;

    private static TestSettings Load()
    {
        var baseDir = AppContext.BaseDirectory;
        var builder = new ConfigurationBuilder()
            .SetBasePath(baseDir)
            .AddJsonFile(Path.Combine("Config", "appsettings.json"), optional: false, reloadOnChange: false)
            .AddJsonFile(Path.Combine("Config", "appsettings.Local.json"), optional: true, reloadOnChange: false)
            .AddEnvironmentVariables(prefix: "WA_");

        var config = builder.Build();
        var section = config.GetSection("TestSettings");
        var settings = section.Get<TestSettings>()
            ?? throw new InvalidOperationException("Missing TestSettings section in configuration.");
        return settings;
    }
}

public sealed class BrowserSettings
{
    public string Name { get; init; } = "chromium";
    public bool Headless { get; init; } = true;
    public int SlowMoMs { get; init; }
    public int ViewportWidth { get; init; } = 1440;
    public int ViewportHeight { get; init; } = 900;
    public int DefaultTimeoutMs { get; init; } = 15000;
    public int NavigationTimeoutMs { get; init; } = 30000;
}

public sealed class ApiSettings
{
    public int TimeoutSeconds { get; init; } = 30;
    public string UserAgent { get; init; } = "WikipediaAutomation/1.0";
}

public sealed class TracingSettings
{
    public bool Enabled { get; init; }
    public string OutputDirectory { get; init; } = "test-results/traces";
}
