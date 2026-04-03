namespace Penn.Search;

using Penn.Routing;

public record SearchIndexDocument(
    string Title,
    string Body,
    UrlPath Url,
    string? Section,
    string Locale,
    int Priority
);
