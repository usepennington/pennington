namespace Penn.Generation;

using System.Collections.Immutable;
using Penn.Routing;

public sealed class BuildReport
{
    public ImmutableList<BuildDiagnostic> Diagnostics { get; }
    public ImmutableList<BrokenLink> BrokenLinks { get; }
    public ImmutableList<ContentRoute> GeneratedPages { get; }
    public ImmutableList<ContentRoute> SkippedPages { get; }
    public ImmutableList<ContentRoute> FailedPages { get; }
    public TimeSpan Duration { get; }

    public bool HasErrors => Diagnostics.Any(d => d is DiagnosticError)
                          || BrokenLinks.Count > 0
                          || FailedPages.Count > 0;

    public int TotalPages => GeneratedPages.Count + SkippedPages.Count + FailedPages.Count;

    public BuildReport(
        ImmutableList<BuildDiagnostic> diagnostics,
        ImmutableList<BrokenLink> brokenLinks,
        ImmutableList<ContentRoute> generatedPages,
        ImmutableList<ContentRoute> skippedPages,
        ImmutableList<ContentRoute> failedPages,
        TimeSpan duration)
    {
        Diagnostics = diagnostics;
        BrokenLinks = brokenLinks;
        GeneratedPages = generatedPages;
        SkippedPages = skippedPages;
        FailedPages = failedPages;
        Duration = duration;
    }
}
