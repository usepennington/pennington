namespace Pennington.Pipeline;

using Routing;

public record MarkdownFileSource(FilePath Path);
public record RazorPageSource(string ComponentType);
public record RedirectSource(UrlPath TargetUrl);
public record ProgrammaticSource(IProgrammaticContentGenerator Generator);

public union ContentSource(MarkdownFileSource, RazorPageSource, RedirectSource, ProgrammaticSource);