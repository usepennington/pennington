namespace Pennington.Generation;

using System.Collections.Immutable;
using System.Linq;
using Diagnostics;
using Routing;

/// <summary>
/// Aggregated result of a static build run, including diagnostics, broken links, and per-page outcomes.
/// </summary>
public sealed class BuildReport
{
    /// <summary>Diagnostics recorded during the build.</summary>
    public ImmutableList<BuildDiagnostic> Diagnostics { get; }

    /// <summary>Broken links discovered by link verification.</summary>
    [Obsolete("Read Diagnostics filtered by source 'content.links/' instead. Scheduled for removal one release later.")]
    public ImmutableList<BrokenLink> BrokenLinks { get; }

    /// <summary>Routes that were successfully written to the output directory.</summary>
    public ImmutableList<ContentRoute> GeneratedPages { get; }

    /// <summary>Routes that were skipped (typically drafts).</summary>
    public ImmutableList<ContentRoute> SkippedPages { get; }

    /// <summary>Routes whose generation failed.</summary>
    public ImmutableList<ContentRoute> FailedPages { get; }

    /// <summary>Total wall-clock duration of the build.</summary>
    public TimeSpan Duration { get; }

    /// <summary>True when the build produced any errors, failed pages, or broken links.</summary>
    public bool HasErrors => Diagnostics.Any(d => d.Severity is DiagnosticSeverity.Error)
#pragma warning disable CS0618
                          || BrokenLinks.Count > 0
#pragma warning restore CS0618
                          || FailedPages.Count > 0;

    /// <summary>Total number of pages considered, including generated, skipped, and failed.</summary>
    public int TotalPages => GeneratedPages.Count + SkippedPages.Count + FailedPages.Count;

    /// <summary>Initializes a completed build report with all captured results.</summary>
    public BuildReport(
        ImmutableList<BuildDiagnostic> diagnostics,
        ImmutableList<BrokenLink> brokenLinks,
        ImmutableList<ContentRoute> generatedPages,
        ImmutableList<ContentRoute> skippedPages,
        ImmutableList<ContentRoute> failedPages,
        TimeSpan duration)
    {
        Diagnostics = diagnostics;
#pragma warning disable CS0618
        BrokenLinks = brokenLinks;
#pragma warning restore CS0618
        GeneratedPages = generatedPages;
        SkippedPages = skippedPages;
        FailedPages = failedPages;
        Duration = duration;
    }

    /// <summary>Writes a human-readable summary of the report to <paramref name="writer"/>.</summary>
    public void WriteTo(TextWriter writer)
    {
        // Summary line
        writer.WriteLine($"Build Complete — {TotalPages} pages in {Duration.TotalSeconds:F1}s");
        writer.WriteLine($"  {GeneratedPages.Count} pages generated");
        if (SkippedPages.Count > 0)
        {
            writer.WriteLine($"  {SkippedPages.Count} pages skipped (draft)");
        }

        if (FailedPages.Count > 0)
        {
            writer.WriteLine($"  {FailedPages.Count} pages failed");
        }

        var warnings = Diagnostics.Count(d => d.Severity is DiagnosticSeverity.Warning);
        if (warnings > 0)
        {
            writer.WriteLine($"  {warnings} warnings");
        }

        writer.WriteLine();

        // Errors section
        var errors = Diagnostics.Where(d => d.Severity is DiagnosticSeverity.Error).ToList();
        if (errors.Count > 0)
        {
            writer.WriteLine("ERRORS");
            foreach (var diag in errors)
            {
                WriteDiagnostic(writer, diag);
            }
            writer.WriteLine();
        }

        // Warnings section
        var warningDiags = Diagnostics.Where(d => d.Severity is DiagnosticSeverity.Warning).ToList();
#pragma warning disable CS0618
        var brokenLinks = BrokenLinks;
#pragma warning restore CS0618
        if (warningDiags.Count > 0 || brokenLinks.Count > 0)
        {
            writer.WriteLine("WARNINGS");
            foreach (var diag in warningDiags)
            {
                if (diag.Route is { } route)
                {
                    writer.WriteLine($"  {route.CanonicalPath}: {diag.Message}");
                }
                else
                {
                    writer.WriteLine($"  {diag.Message}");
                }
            }
            if (brokenLinks.Count > 0)
            {
                writer.WriteLine($"  {brokenLinks.Count} broken links found:");
                foreach (var link in brokenLinks)
                {
                    writer.WriteLine($"    {link.SourcePage.CanonicalPath} links to {link.Url} ({link.Reason})");
                }
            }
            writer.WriteLine();
        }
    }

    private static void WriteDiagnostic(TextWriter writer, BuildDiagnostic diag)
    {
        if (diag.Route is { } route)
        {
            writer.WriteLine($"  {route.CanonicalPath}");
            writer.WriteLine($"    {diag.Message}");
            if (route.SourceFile is { } routeSource)
            {
                writer.WriteLine($"    Source: {routeSource}");
            }
            else if (diag.SourceFile is { } diagSource)
            {
                writer.WriteLine($"    File: {diagSource}");
            }
        }
        else if (diag.SourceFile is { } sourceFile)
        {
            writer.WriteLine($"  {sourceFile}");
            writer.WriteLine($"    {diag.Message}");
        }
        else
        {
            writer.WriteLine($"  {diag.Message}");
        }

        if (diag.Exception is { } ex)
        {
            writer.WriteLine($"    Exception: {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>Returns the human-readable summary as a single formatted string.</summary>
    public string ToFormattedString()
    {
        using var writer = new StringWriter();
        WriteTo(writer);
        return writer.ToString();
    }
}