namespace SpectreConsoleExample;

using Pennington.FrontMatter;

/// <summary>
/// Front matter for Spectre.Console and CLI documentation pages.
/// </summary>
public record SpectreDocFrontMatter : IFrontMatter, ITaggable,
    ISectionable, IOrderable, IRedirectable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public string? Section { get; init; }
    public string? Uid { get; init; }
    public int Order { get; init; } = int.MaxValue;
    public string? RedirectUrl { get; init; }
}

/// <summary>
/// Front matter for blog posts in the Spectre.Console documentation.
/// </summary>
public record SpectreBlogFrontMatter : IFrontMatter, ITaggable
{
    public string Title { get; init; } = "";
    public string? Author { get; init; }
    public string? Description { get; init; }
    public DateTime? Date { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public string? Series { get; init; }
    public string? Uid { get; init; }
    public string? Repository { get; init; }
}