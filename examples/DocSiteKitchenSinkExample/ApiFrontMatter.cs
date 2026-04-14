namespace DocSiteKitchenSinkExample;

using Pennington.FrontMatter;

/// <summary>
/// Custom front-matter record used by the "multiple content sources" how-to.
/// Implements the same capability interfaces as <c>DocSiteFrontMatter</c>
/// plus an API-specific <see cref="Namespace"/> and <see cref="Stability"/>
/// pair so reference pages can expose a per-API namespace and stability
/// badge.
/// </summary>
/// <remarks>
/// Kept as a standalone record so tutorials can target it with
/// <c>T:DocSiteKitchenSinkExample.ApiFrontMatter</c>. Declaring a record
/// that implements <see cref="IFrontMatter"/> with a small handful of
/// capability interfaces is the canonical "write your own front matter"
/// pattern referenced by the front-matter how-to.
/// </remarks>
public record ApiFrontMatter : IFrontMatter, ITaggable, ISectionable, IOrderable, IRedirectable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public int Order { get; init; } = int.MaxValue;
    public string? RedirectUrl { get; init; }
    public string? SectionLabel { get; init; }
    public string? Uid { get; init; }
    public bool Search { get; init; } = true;
    public bool Llms { get; init; } = true;

    /// <summary>API namespace (e.g. <c>Pennington.Highlighting</c>).</summary>
    public string? Namespace { get; init; }

    /// <summary>Stability classification — <c>stable</c>, <c>preview</c>, or <c>experimental</c>.</summary>
    public string Stability { get; init; } = "stable";
}
