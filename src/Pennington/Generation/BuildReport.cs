namespace Pennington.Generation;

using System.Collections.Immutable;
using System.Linq;
using Diagnostics;
using Routing;

public sealed class BuildReport
{
    public ImmutableList<BuildDiagnostic> Diagnostics { get; }
    public ImmutableList<BrokenLink> BrokenLinks { get; }
    public ImmutableList<ContentRoute> GeneratedPages { get; }
    public ImmutableList<ContentRoute> SkippedPages { get; }
    public ImmutableList<ContentRoute> FailedPages { get; }
    public TimeSpan Duration { get; }

    public bool HasErrors => Diagnostics.Any(d => d.Severity is DiagnosticSeverity.Error)
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

        var warnings = Diagnostics.Count(d => d.Severity is DiagnosticSeverity.Warning);
        if (warnings > 0)
            writer.WriteLine($"  {warnings} warnings");

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
        if (warningDiags.Count > 0 || BrokenLinks.Count > 0)
        {
            writer.WriteLine("WARNINGS");
            foreach (var diag in warningDiags)
            {
                if (diag.Route is { } route)
                    writer.WriteLine($"  {route.CanonicalPath}: {diag.Message}");
                else
                    writer.WriteLine($"  {diag.Message}");
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

    private static void WriteDiagnostic(TextWriter writer, BuildDiagnostic diag)
    {
        if (diag.Route is { } route)
        {
            writer.WriteLine($"  {route.CanonicalPath}");
            writer.WriteLine($"    {diag.Message}");
            if (route.SourceFile is { } routeSource)
                writer.WriteLine($"    Source: {routeSource}");
            else if (diag.SourceFile is { } diagSource)
                writer.WriteLine($"    File: {diagSource}");
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
    }

    public string ToFormattedString()
    {
        using var writer = new StringWriter();
        WriteTo(writer);
        return writer.ToString();
    }
}