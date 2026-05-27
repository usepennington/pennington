namespace Pennington.Pipeline;

using AngleSharp.Dom;
using Content;
using Routing;
using Search;

/// <summary>
/// Origin information for a <see cref="RenderedPage"/>: the page came from a
/// markdown source (carrying a <see cref="ParsedItem"/> with front matter +
/// derived metadata), from a Razor / programmatic / API-symbol source (no
/// parsed item available), or from an endpoint opt-in
/// (<see cref="LlmsTxt.LlmsTxtEndpointExtensions"/>) where the link target is
/// the endpoint URL itself and no HTML is fetched.
/// </summary>
/// <param name="Parsed">Enriched parsed item for the page, including derived metadata.</param>
public record MarkdownOrigin(ParsedItem Parsed);

/// <summary>Page rendered from a Razor component, programmatic generator, or any other non-markdown source with no <see cref="ParsedItem"/>.</summary>
public record RazorOrigin();

/// <summary>Endpoint opted into llms.txt via <c>WithLlmsTxtEntry</c>; the direct URL is the link target and no HTML is captured.</summary>
/// <param name="DirectUrl">URL of the endpoint the llms.txt index should point at.</param>
public record EndpointOrigin(string DirectUrl);

/// <summary>Where a <see cref="RenderedPage"/> originated and what metadata is available for it.</summary>
#if NET11_0_OR_GREATER
public union PageOrigin(MarkdownOrigin, RazorOrigin, EndpointOrigin);
#else
[System.Runtime.CompilerServices.Union]
public readonly struct PageOrigin : System.Runtime.CompilerServices.IUnion
{
    /// <summary>Wrapped case instance; inspect via pattern matching on the case types.</summary>
    public object? Value { get; }
    /// <summary>Wraps a <see cref="MarkdownOrigin"/>.</summary>
    public PageOrigin(MarkdownOrigin value) { Value = value; }
    /// <summary>Wraps a <see cref="RazorOrigin"/>.</summary>
    public PageOrigin(RazorOrigin value) { Value = value; }
    /// <summary>Wraps an <see cref="EndpointOrigin"/>.</summary>
    public PageOrigin(EndpointOrigin value) { Value = value; }
    /// <summary>Implicit conversion from <see cref="MarkdownOrigin"/>.</summary>
    public static implicit operator PageOrigin(MarkdownOrigin value) => new(value);
    /// <summary>Implicit conversion from <see cref="RazorOrigin"/>.</summary>
    public static implicit operator PageOrigin(RazorOrigin value) => new(value);
    /// <summary>Implicit conversion from <see cref="EndpointOrigin"/>.</summary>
    public static implicit operator PageOrigin(EndpointOrigin value) => new(value);
}
#endif

/// <summary>
/// One page in the projected corpus: route + TOC entry + origin metadata, plus
/// the post-pipeline HTML and a parsed AngleSharp element to aggregate over.
/// <para>
/// Produced by <see cref="ISiteProjection"/> once per route per file-watch
/// generation and shared across every corpus aggregator (search, llms.txt,
/// link audit). Consumers pattern-match on <see cref="Origin"/> to recover
/// front-matter / derived metadata where it exists.
/// </para>
/// <para>
/// <see cref="Content"/> is owned by a per-page AngleSharp browsing context —
/// the projection holds the context for the page's lifetime. Treat
/// <see cref="Content"/> as read-only: mutating it corrupts other consumers'
/// views.
/// </para>
/// </summary>
/// <param name="Route">Canonical route for the page.</param>
/// <param name="Toc">TOC entry that drove the page's inclusion.</param>
/// <param name="Origin">Origin information (markdown / razor / endpoint).</param>
/// <param name="Html">Post-pipeline HTML of the selector-matched content element; empty for endpoint pages.</param>
/// <param name="Content">Parsed content element from the post-pipeline HTML; null for endpoint pages.</param>
/// <param name="Sections">Lazy heading-section split of <see cref="Content"/>; empty for endpoint pages.</param>
public sealed record RenderedPage(
    ContentRoute Route,
    ContentTocItem Toc,
    PageOrigin Origin,
    string Html,
    IElement? Content,
    Lazy<IReadOnlyList<HeadingSection>> Sections);
