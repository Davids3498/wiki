using System.Text.Json.Serialization;

namespace WikipediaAutomation.Models;

public sealed class ParseTextResponse
{
    [JsonPropertyName("parse")]
    public ParseTextPayload? Parse { get; init; }
}

public sealed class ParseTextPayload
{
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("pageid")]
    public long PageId { get; init; }

    [JsonPropertyName("text")]
    public ParseTextValue? Text { get; init; }
}

public sealed class ParseTextValue
{
    [JsonPropertyName("*")]
    public string Html { get; init; } = string.Empty;
}
