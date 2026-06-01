using System.Collections.Immutable;
using Pennington.Generation;
using Pennington.Routing;

namespace Pennington.Tests.Generation;

public class BuildReportOutputTests
{
    private static ContentRoute MakeRoute(string path, string? source = null) => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html"),
        SourceFile = source is not null ? new FilePath(source) : (FilePath?)null
    };

    private static BuildReport MakeReport(
        ImmutableList<BuildDiagnostic>? diagnostics = null,
        ImmutableList<ContentRoute>? generatedPages = null,
        ImmutableList<ContentRoute>? skippedPages = null,
        ImmutableList<ContentRoute>? failedPages = null,
        TimeSpan? duration = null) => new(
            diagnostics ?? [],
            generatedPages ?? [],
            skippedPages ?? [],
            failedPages ?? [],
            duration ?? TimeSpan.FromSeconds(1));

    // Mirrors what LinkAuditor emits: a content.links/ warning diagnostic per broken link.
    private static BuildDiagnostic BrokenLinkDiag(string page, string url, LinkType type, string reason) =>
        new(DiagnosticSeverity.Warning, MakeRoute(page),
            $"Broken link to {url} ({reason})", SourceFile: $"content.links/{type}/{url}");

    [Fact]
    public void CleanBuild_ShowsGeneratedPages_NoErrorsOrWarnings()
    {
        var report = MakeReport(
            generatedPages: [MakeRoute("/a"), MakeRoute("/b"), MakeRoute("/c")],
            duration: TimeSpan.FromSeconds(1.5));

        var output = report.ToFormattedString();

        output.ShouldContain("3 pages generated");
        output.ShouldNotContain("ERRORS");
        output.ShouldNotContain("WARNINGS");
        output.ShouldNotContain("pages skipped");
        output.ShouldNotContain("pages failed");
    }

    [Fact]
    public void BuildWithErrors_ShowsErrorsSection()
    {
        var errorRoute = MakeRoute("/docs/api/broken-page", "Content/Docs/api/broken-page.md");
        var diagnostics = ImmutableList.Create(
            new BuildDiagnostic(DiagnosticSeverity.Error, errorRoute, "HTTP 500: NullReferenceException in Render"));

        var report = MakeReport(
            diagnostics: diagnostics,
            generatedPages: [MakeRoute("/a")],
            failedPages: [errorRoute],
            duration: TimeSpan.FromSeconds(3.2));

        var output = report.ToFormattedString();

        output.ShouldContain("ERRORS");
        output.ShouldContain("/docs/api/broken-page");
        output.ShouldContain("HTTP 500: NullReferenceException in Render");
        output.ShouldContain("Source: Content/Docs/api/broken-page.md");
        output.ShouldContain("1 pages failed");
    }

    [Fact]
    public void BuildWithWarnings_ShowsWarningsSection()
    {
        var diagnostics = ImmutableList.Create(
            new BuildDiagnostic(DiagnosticSeverity.Warning, MakeRoute("/docs/old-page"), "redirect target not found"));

        var report = MakeReport(
            diagnostics: diagnostics,
            generatedPages: [MakeRoute("/a"), MakeRoute("/b")]);

        var output = report.ToFormattedString();

        output.ShouldContain("WARNINGS");
        output.ShouldContain("/docs/old-page/: redirect target not found");
        output.ShouldContain("1 warnings");
    }

    [Fact]
    public void BuildWithBrokenLinks_ShowsInWarnings_AndFailsBuild()
    {
        var diagnostics = ImmutableList.Create(
            BrokenLinkDiag("/docs/setup", "/docs/install", LinkType.Internal, "404"),
            BrokenLinkDiag("/blog/post", "/missing-image.png", LinkType.Image, "404"));

        var report = MakeReport(
            diagnostics: diagnostics,
            generatedPages: [MakeRoute("/a")]);

        // Broken links are content.links/ warning diagnostics — they render under
        // WARNINGS and still fail the build (the relocated HasErrors gate).
        report.HasErrors.ShouldBeTrue();

        var output = report.ToFormattedString();
        output.ShouldContain("WARNINGS");
        output.ShouldContain("/docs/setup/: Broken link to /docs/install (404)");
        output.ShouldContain("/blog/post/: Broken link to /missing-image.png (404)");
    }

    [Fact]
    public void BuildWithSkippedDrafts_ShowsSkippedCount()
    {
        var report = MakeReport(
            generatedPages: [MakeRoute("/a"), MakeRoute("/b")],
            skippedPages: [MakeRoute("/draft-1"), MakeRoute("/draft-2")]);

        var output = report.ToFormattedString();

        output.ShouldContain("2 pages skipped (draft)");
        output.ShouldContain("4 pages in");
    }

    [Fact]
    public void MixedBuild_AllSectionsAppear()
    {
        var errorRoute = MakeRoute("/broken", "Content/broken.md");
        var diagnostics = ImmutableList.Create(
            new BuildDiagnostic(DiagnosticSeverity.Error, errorRoute, "render failed"),
            new BuildDiagnostic(DiagnosticSeverity.Warning, MakeRoute("/old"), "deprecated"),
            BrokenLinkDiag("/page", "/missing", LinkType.Internal, "404"));

        var report = MakeReport(
            diagnostics: diagnostics,
            generatedPages: [MakeRoute("/a"), MakeRoute("/b")],
            skippedPages: [MakeRoute("/draft")],
            failedPages: [errorRoute],
            duration: TimeSpan.FromSeconds(5.7));

        var output = report.ToFormattedString();

        // Summary
        output.ShouldContain("4 pages in 5.7s");
        output.ShouldContain("2 pages generated");
        output.ShouldContain("1 pages skipped (draft)");
        output.ShouldContain("1 pages failed");
        output.ShouldContain("2 warnings");

        // Errors section
        output.ShouldContain("ERRORS");
        output.ShouldContain("/broken");
        output.ShouldContain("render failed");
        output.ShouldContain("Source: Content/broken.md");

        // Warnings section (deprecation + broken link both render here)
        output.ShouldContain("WARNINGS");
        output.ShouldContain("/old/: deprecated");
        output.ShouldContain("/page/: Broken link to /missing (404)");
    }

}