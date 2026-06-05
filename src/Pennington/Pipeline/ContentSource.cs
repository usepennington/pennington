namespace Pennington.Pipeline;

using Routing;

/// <summary>Content sourced from a file on disk, tagged with a format key that selects its parser/renderer.</summary>
/// <param name="Path">Absolute path to the source file.</param>
/// <param name="Format">Format key (e.g. <c>"markdown"</c>, <c>"cook"</c>) selecting the parser and renderer.</param>
public record FileSource(FilePath Path, string Format)
{
    /// <summary>True when <see cref="Format"/> is one of the built-in markdown dispatch keys (see <see cref="MarkdownFormat"/>).</summary>
    public bool IsMarkdown => MarkdownFormat.Matches(Format);
}

/// <summary>Content rendered by a Razor page/component.</summary>
/// <param name="ComponentType">Fully qualified name of the component type.</param>
public record RazorPageSource(string ComponentType);

/// <summary>A route that redirects to another URL.</summary>
/// <param name="TargetUrl">Destination URL for the redirect.</param>
public record RedirectSource(UrlPath TargetUrl);

/// <summary>
/// Marker source for routes whose content is produced by a live HTTP endpoint
/// (e.g., the SPA data endpoint). These items exist so the build crawler
/// discovers the URL and fetches it through the live pipeline — they do not
/// participate in parse/render, are not redirects, and do not appear in the
/// sitemap.
/// </summary>
public record EndpointSource();

/// <summary>
/// A markdown file that contributes only to the llms.txt index and its sidecar
/// markdown — no HTML page is emitted, and the route is excluded from
/// navigation, sitemap, RSS, and the search index. Conventionally produced by
/// <see cref="Content.MarkdownContentService{T}"/> when it sees a <c>*.llms.md</c>
/// file: the discovered route uses the slug with the <c>.llms</c> suffix
/// stripped, and downstream stages key off the source type to skip HTML.
/// </summary>
/// <param name="Path">Absolute path to the markdown file.</param>
/// <param name="Format">Markdown dispatch key (see <see cref="MarkdownFormat"/>) of the source that produced this file, selecting the parser for its front-matter type.</param>
public record LlmsOnlySource(FilePath Path, string Format);

/// <summary>Union of all ways content can be sourced for a route.</summary>
#if NET11_0_OR_GREATER
public union ContentSource(FileSource, RazorPageSource, RedirectSource, EndpointSource, LlmsOnlySource);
#else
[System.Runtime.CompilerServices.Union]
public readonly struct ContentSource : System.Runtime.CompilerServices.IUnion
{
    /// <summary>Wrapped case instance; inspect via pattern matching on the case types.</summary>
    public object? Value { get; }
    /// <summary>Wraps a <see cref="FileSource"/>.</summary>
    public ContentSource(FileSource value) { Value = value; }
    /// <summary>Wraps a <see cref="RazorPageSource"/>.</summary>
    public ContentSource(RazorPageSource value) { Value = value; }
    /// <summary>Wraps a <see cref="RedirectSource"/>.</summary>
    public ContentSource(RedirectSource value) { Value = value; }
    /// <summary>Wraps an <see cref="EndpointSource"/>.</summary>
    public ContentSource(EndpointSource value) { Value = value; }
    /// <summary>Wraps an <see cref="LlmsOnlySource"/>.</summary>
    public ContentSource(LlmsOnlySource value) { Value = value; }
    /// <summary>Implicit conversion from <see cref="FileSource"/>.</summary>
    public static implicit operator ContentSource(FileSource value) => new(value);
    /// <summary>Implicit conversion from <see cref="RazorPageSource"/>.</summary>
    public static implicit operator ContentSource(RazorPageSource value) => new(value);
    /// <summary>Implicit conversion from <see cref="RedirectSource"/>.</summary>
    public static implicit operator ContentSource(RedirectSource value) => new(value);
    /// <summary>Implicit conversion from <see cref="EndpointSource"/>.</summary>
    public static implicit operator ContentSource(EndpointSource value) => new(value);
    /// <summary>Implicit conversion from <see cref="LlmsOnlySource"/>.</summary>
    public static implicit operator ContentSource(LlmsOnlySource value) => new(value);
}
#endif