namespace BeyondRemoteContentExample;

using Pennington.FrontMatter;

/// <summary>
/// Minimal front matter for an API-sourced release. The renderer only needs a title;
/// every other <see cref="IFrontMatter"/> member keeps its capability default.
/// </summary>
/// <param name="Title">Release title, shown in the browser tab and headings.</param>
public sealed record ReleaseFrontMatter(string Title) : IFrontMatter;
