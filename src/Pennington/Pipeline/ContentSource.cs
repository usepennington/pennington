namespace Pennington.Pipeline;

using Routing;

public record MarkdownFileSource(FilePath Path);
public record RazorPageSource(string ComponentType);
public record RedirectSource(UrlPath TargetUrl);
public record ProgrammaticSource(IProgrammaticContentGenerator Generator);

/// <summary>
/// Marker source for routes whose content is produced by a live HTTP endpoint
/// (e.g., the SPA data endpoint). These items exist so the build crawler
/// discovers the URL and fetches it through the live pipeline — they do not
/// participate in parse/render, are not redirects, and do not appear in the
/// sitemap.
/// </summary>
public record EndpointSource();

public union ContentSource(MarkdownFileSource, RazorPageSource, RedirectSource, ProgrammaticSource, EndpointSource);