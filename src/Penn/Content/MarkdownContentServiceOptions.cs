namespace Penn.Content;

using Penn.Routing;

/// <summary>
/// Configuration for a markdown content source.
/// </summary>
public sealed class MarkdownContentServiceOptions
{
    public required FilePath ContentPath { get; init; }
    public UrlPath BasePageUrl { get; init; } = new("/");
    public string? Section { get; init; }
    public string FilePattern { get; init; } = "*.md";
    public string Locale { get; init; } = "";
    public int SearchPriority { get; init; } = 10;
}
