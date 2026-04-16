namespace Pennington.Pipeline;

using Routing;

/// <summary>Content sourced from a markdown file on disk.</summary>
/// <param name="Path">Absolute path to the markdown file.</param>
public record MarkdownFileSource(FilePath Path);

/// <summary>Content rendered by a Razor page/component.</summary>
/// <param name="ComponentType">Fully qualified name of the component type.</param>
public record RazorPageSource(string ComponentType);

/// <summary>A route that redirects to another URL.</summary>
/// <param name="TargetUrl">Destination URL for the redirect.</param>
public record RedirectSource(UrlPath TargetUrl);

/// <summary>Content produced programmatically by a generator.</summary>
/// <param name="Generator">Generator that produces the content on demand.</param>
public record ProgrammaticSource(IProgrammaticContentGenerator Generator);

/// <summary>
/// Marker source for routes whose content is produced by a live HTTP endpoint
/// (e.g., the SPA data endpoint). These items exist so the build crawler
/// discovers the URL and fetches it through the live pipeline — they do not
/// participate in parse/render, are not redirects, and do not appear in the
/// sitemap.
/// </summary>
public record EndpointSource();

/// <summary>Union of all ways content can be sourced for a route.</summary>
public union ContentSource(MarkdownFileSource, RazorPageSource, RedirectSource, ProgrammaticSource, EndpointSource);
