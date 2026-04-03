namespace Penn.Generation;

using System.Collections.Immutable;
using System.Linq;
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

    public void WriteTo(TextWriter writer)
    {
        // Summary line
        writer.WriteLine($"Build Complete — {TotalPages} pages in {Duration.TotalSeconds:F1}s");
        writer.WriteLine($"  {GeneratedPages.Count} pages generated");
        if (SkippedPages.Count > 0)
            writer.WriteLine($"  {SkippedPages.Count} pages skipped (draft)");
        if (FailedPages.Count > 0)
            writer.WriteLine($"  {FailedPages.Count} pages failed");

        var warnings = Diagnostics.Count(d => d is DiagnosticWarning);
        if (warnings > 0)
            writer.WriteLine($"  {warnings} warnings");

        writer.WriteLine();

        // Errors section
        var errors = Diagnostics.Where(d => d is DiagnosticError).ToList();
        if (errors.Count > 0)
        {
            writer.WriteLine("ERRORS");
            foreach (var diag in errors)
            {
                writer.WriteLine($"  {diag.Route.CanonicalPath}");
                writer.WriteLine($"    {diag.Message}");
                if (diag.Route.SourceFile is { } source)
                    writer.WriteLine($"    Source: {source}");
            }
            writer.WriteLine();
        }

        // Warnings section
        var warningDiags = Diagnostics.Where(d => d is DiagnosticWarning).ToList();
        if (warningDiags.Count > 0 || BrokenLinks.Count > 0)
        {
            writer.WriteLine("WARNINGS");
            foreach (var diag in warningDiags)
            {
                writer.WriteLine($"  {diag.Route.CanonicalPath}: {diag.Message}");
            }
            if (BrokenLinks.Count > 0)
            {
                writer.WriteLine($"  {BrokenLinks.Count} broken links found:");
                foreach (var link in BrokenLinks)
                {
                    writer.WriteLine($"    {link.SourcePage.CanonicalPath} links to {link.Url} ({link.Reason})");
                }
            }
            writer.WriteLine();
        }
    }

    public string ToFormattedString()
    {
        using var writer = new StringWriter();
        WriteTo(writer);
        return writer.ToString();
    }
}
