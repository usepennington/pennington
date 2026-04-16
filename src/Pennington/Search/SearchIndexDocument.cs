namespace Pennington.Search;

/// <summary>A single document entry in the search index.</summary>
/// <param name="Title">Document title.</param>
/// <param name="Body">Plain-text body used for full-text matching.</param>
/// <param name="Url">Canonical URL for the document.</param>
/// <param name="SectionLabel">Optional section label shown in results.</param>
/// <param name="Locale">Locale code this document belongs to.</param>
/// <param name="Priority">Relative weight used to rank results; higher wins.</param>
public record SearchIndexDocument(
    string Title,
    string Body,
    string Url,
    string? SectionLabel,
    string Locale,
    int Priority
);
