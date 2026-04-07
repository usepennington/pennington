namespace Penn.Search;

public record SearchIndexDocument(
    string Title,
    string Body,
    string Url,
    string? Section,
    string Locale,
    int Priority
);
