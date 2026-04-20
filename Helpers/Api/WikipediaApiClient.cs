using System.Net.Http.Json;
using System.Text.Json;
using WikipediaAutomation.Config;
using WikipediaAutomation.Models;

namespace WikipediaAutomation.Helpers.Api;

public sealed class WikipediaApiClient : IWikipediaApiClient, IDisposable
{
    private readonly HttpClient _http;
    private readonly Uri _apiBase;
    private readonly bool _ownsClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public WikipediaApiClient(TestSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _apiBase = settings.ApiBaseUri;
        _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(settings.Api.TimeoutSeconds),
        };
        _http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", settings.Api.UserAgent);
        _http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        _ownsClient = true;
    }

    // Injection-friendly constructor for tests that want to stub HttpMessageHandler.
    public WikipediaApiClient(HttpClient httpClient, Uri apiBase)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiBase = apiBase ?? throw new ArgumentNullException(nameof(apiBase));
        _ownsClient = false;
    }

    public async Task<IReadOnlyList<SectionInfo>> GetSectionsAsync(string pageTitle, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pageTitle);

        var uri = BuildUri(new Dictionary<string, string?>
        {
            ["action"] = "parse",
            ["page"] = pageTitle,
            ["prop"] = "sections",
            ["format"] = "json",
            ["formatversion"] = "1",
        });

        using var response = await _http.GetAsync(uri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ParseSectionsResponse>(JsonOptions, cancellationToken).ConfigureAwait(false);
        var sections = payload?.Parse?.Sections
            ?? throw new InvalidOperationException($"Parse API returned no sections for page '{pageTitle}'.");
        return sections;
    }

    public async Task<string> GetSectionHtmlAsync(string pageTitle, string sectionIndex, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pageTitle);
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionIndex);

        var uri = BuildUri(new Dictionary<string, string?>
        {
            ["action"] = "parse",
            ["page"] = pageTitle,
            ["section"] = sectionIndex,
            ["prop"] = "text",
            ["format"] = "json",
            ["formatversion"] = "1",
            ["disablelimitreport"] = "1",
            ["disableeditsection"] = "1",
        });

        using var response = await _http.GetAsync(uri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ParseTextResponse>(JsonOptions, cancellationToken).ConfigureAwait(false);
        return payload?.Parse?.Text?.Html
            ?? throw new InvalidOperationException($"Parse API returned no text for section '{sectionIndex}' on page '{pageTitle}'.");
    }

    public async Task<string> GetSectionHtmlByTitleAsync(string pageTitle, string sectionTitle, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionTitle);

        var sections = await GetSectionsAsync(pageTitle, cancellationToken).ConfigureAwait(false);
        var match = sections.FirstOrDefault(s =>
            string.Equals(s.Line, sectionTitle, StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            var known = string.Join(", ", sections.Select(s => $"\"{s.Line}\""));
            throw new InvalidOperationException(
                $"Section '{sectionTitle}' not found on page '{pageTitle}'. Available: {known}.");
        }

        return await GetSectionHtmlAsync(pageTitle, match.Index, cancellationToken).ConfigureAwait(false);
    }

    private Uri BuildUri(IDictionary<string, string?> query)
    {
        var builder = new UriBuilder(_apiBase);
        var encoded = string.Join("&", query
            .Where(kv => kv.Value is not null)
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}"));
        builder.Query = encoded;
        return builder.Uri;
    }

    public void Dispose()
    {
        if (_ownsClient)
        {
            _http.Dispose();
        }
    }
}
