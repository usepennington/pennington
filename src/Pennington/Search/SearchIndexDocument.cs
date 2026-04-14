namespace Pennington.Search;

public record SearchIndexDocument(
    string Title,
    string Body,
    string Url,
    string? SectionLabel,
    string Locale,
    int Priority
);