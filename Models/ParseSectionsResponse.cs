using System.Text.Json.Serialization;

namespace WikipediaAutomation.Models;

public sealed class ParseSectionsResponse
{
    [JsonPropertyName("parse")]
    public ParseSectionsPayload? Parse { get; init; }
}

public sealed class ParseSectionsPayload
{
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("pageid")]
    public long PageId { get; init; }

    [JsonPropertyName("sections")]
    public IReadOnlyList<SectionInfo> Sections { get; init; } = Array.Empty<SectionInfo>();
}

public sealed class SectionInfo
{
    [JsonPropertyName("toclevel")]
    public int TocLevel { get; init; }

    [JsonPropertyName("level")]
    public string Level { get; init; } = string.Empty;

    [JsonPropertyName("line")]
    public string Line { get; init; } = string.Empty;

    [JsonPropertyName("number")]
    public string Number { get; init; } = string.Empty;

    [JsonPropertyName("index")]
    public string Index { get; init; } = string.Empty;

    [JsonPropertyName("anchor")]
    public string Anchor { get; init; } = string.Empty;
}
