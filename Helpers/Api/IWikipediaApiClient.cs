using WikipediaAutomation.Models;

namespace WikipediaAutomation.Helpers.Api;

public interface IWikipediaApiClient
{
    Task<IReadOnlyList<SectionInfo>> GetSectionsAsync(string pageTitle, CancellationToken cancellationToken = default);

    Task<string> GetSectionHtmlAsync(string pageTitle, string sectionIndex, CancellationToken cancellationToken = default);

    /// <summary>
    /// Convenience: resolves the section index by heading title, then fetches its HTML.
    /// Matches by case-insensitive heading line equality.
    /// </summary>
    Task<string> GetSectionHtmlByTitleAsync(string pageTitle, string sectionTitle, CancellationToken cancellationToken = default);
}
