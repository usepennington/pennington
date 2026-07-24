namespace Pennington.Pipeline;

using Content;
using Routing;
using Search;

/// <summary>
/// Origin information for a <see cref="RenderedPage"/>: the page came from a
/// markdown source (carrying a <see cref="ParsedItem"/> with front matter +
/// derived metadata) or from an endpoint opt-in
/// (<see cref="LlmsTxt.LlmsTxtEndpointExtensions"/>) where the link target is
/// the endpoint URL itself and no HTML is fetched. Razor / programmatic
/// sources carry no origin metadata; see <see cref="RenderedPage.Origin"/>.
/// </summary>
/// <param name="Parsed">Enriched parsed item for the page, including derived metadata.</param>
public record MarkdownOrigin(ParsedItem Parsed);

/// <summary>Endpoint opted into llms.txt via <c>WithLlmsTxtEntry</c>; the direct URL is the link target and no HTML is captured.</summary>
/// <param name="DirectUrl">URL of the endpoint the llms.txt index should point at.</param>
public record EndpointOrigin(string DirectUrl);

/// <summary>Where a <see cref="RenderedPage"/> originated and what metadata is available for it.</summary>
#if NET11_0_OR_GREATER
public union PageOrigin(MarkdownOrigin, EndpointOrigin);
#else
[System.Runtime.CompilerServices.Union]
public readonly struct PageOrigin : System.Runtime.CompilerServices.IUnion
{
    /// <summary>Wrapped case instance; inspect via pattern matching on the case types.</summary>
    public object? Value { get; }

    /// <summary>Wraps a <see cref="MarkdownOrigin"/>.</summary>
    public PageOrigin(MarkdownOrigin value) { Value = value; }

    /// <summary>Wraps an <see cref="EndpointOrigin"/>.</summary>
    public PageOrigin(EndpointOrigin value) { Value = value; }

    /// <summary>Implicit conversion from <see cref="MarkdownOrigin"/>.</summary>
    public static implicit operator PageOrigin(MarkdownOrigin value) => new(value);

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
/// front-matter / derived metadata where it exists. A <c>null</c>
/// <see cref="Origin"/> means a Razor / programmatic page with no parsed
/// item — the HTML is still available, just without front-matter context.
/// </para>
/// <para>
/// No parsed DOM is retained. Holding an AngleSharp <c>IElement</c> keeps its whole
/// owning document alive, so a corpus-sized cache of them costs roughly ten times
/// what <see cref="Html"/> costs — enough to exhaust memory on large sites. Anything
/// derived from the DOM is either captured here as value data
/// (<see cref="Sections"/>) or re-parsed from <see cref="Html"/> on demand by the
/// consumer that needs it, which keeps the parse transient.
/// </para>
/// </summary>
/// <param name="Route">Canonical route for the page.</param>
/// <param name="Toc">TOC entry that drove the page's inclusion.</param>
/// <param name="Origin">Origin information (markdown / endpoint), or <c>null</c> for Razor / programmatic pages.</param>
/// <param name="Html">Post-pipeline HTML of the selector-matched content element; empty for endpoint pages.</param>
/// <param name="HasContent">Whether the page produced a content element; <c>false</c> for endpoint pages.</param>
/// <param name="Sections">Heading-section split of the page content; empty for endpoint pages.</param>
public sealed record RenderedPage(
    ContentRoute Route,
    ContentTocItem Toc,
    PageOrigin? Origin,
    string Html,
    bool HasContent,
    IReadOnlyList<HeadingSection> Sections);