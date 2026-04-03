using System.Collections.Immutable;
using Penn.Generation;
using Penn.Routing;

namespace Penn.Tests.Generation;

public class BuildReportOutputTests
{
    private static ContentRoute MakeRoute(string path, string? source = null) => new()
    {
        CanonicalPath = new UrlPath(path),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html"),
        SourceFile = source is not null ? new FilePath(source) : (FilePath?)null
    };

    private static BuildReport MakeReport(
        ImmutableList<BuildDiagnostic>? diagnostics = null,
        ImmutableList<BrokenLink>? brokenLinks = null,
        ImmutableList<ContentRoute>? generatedPages = null,
        ImmutableList<ContentRoute>? skippedPages = null,
        ImmutableList<ContentRoute>? failedPages = null,
        TimeSpan? duration = null) => new(
            diagnostics ?? [],
            brokenLinks ?? [],
            generatedPages ?? [],
            skippedPages ?? [],
            failedPages ?? [],
            duration ?? TimeSpan.FromSeconds(1));

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
            new BuildDiagnostic(new DiagnosticError(errorRoute, "HTTP 500: NullReferenceException in Render", null)));

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
            new BuildDiagnostic(new DiagnosticWarning(MakeRoute("/docs/old-page"), "redirect target not found")));

        var report = MakeReport(
            diagnostics: diagnostics,
            generatedPages: [MakeRoute("/a"), MakeRoute("/b")]);

        var output = report.ToFormattedString();

        output.ShouldContain("WARNINGS");
        output.ShouldContain("/docs/old-page: redirect target not found");
        output.ShouldContain("1 warnings");
    }

    [Fact]
    public void BuildWithBrokenLinks_ShowsLinkDetails()
    {
        var brokenLinks = ImmutableList.Create(
            new BrokenLink(MakeRoute("/docs/setup"), "/docs/install", LinkType.Internal, "404"),
            new BrokenLink(MakeRoute("/blog/post"), "/missing-image.png", LinkType.Image, "404"));

        var report = MakeReport(
            brokenLinks: brokenLinks,
            generatedPages: [MakeRoute("/a")]);

        var output = report.ToFormattedString();

        output.ShouldContain("WARNINGS");
        output.ShouldContain("2 broken links found:");
        output.ShouldContain("/docs/setup links to /docs/install (404)");
        output.ShouldContain("/blog/post links to /missing-image.png (404)");
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
            new BuildDiagnostic(new DiagnosticError(errorRoute, "render failed", null)),
            new BuildDiagnostic(new DiagnosticWarning(MakeRoute("/old"), "deprecated")));
        var brokenLinks = ImmutableList.Create(
            new BrokenLink(MakeRoute("/page"), "/missing", LinkType.Internal, "404"));

        var report = MakeReport(
            diagnostics: diagnostics,
            brokenLinks: brokenLinks,
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
        output.ShouldContain("1 warnings");

        // Errors section
        output.ShouldContain("ERRORS");
        output.ShouldContain("/broken");
        output.ShouldContain("render failed");
        output.ShouldContain("Source: Content/broken.md");

        // Warnings section
        output.ShouldContain("WARNINGS");
        output.ShouldContain("/old: deprecated");
        output.ShouldContain("1 broken links found:");
        output.ShouldContain("/page links to /missing (404)");
    }

    [Fact]
    public void DurationFormatting_ShowsOneDecimalPlace()
    {
        var report = MakeReport(
            generatedPages: [MakeRoute("/a")],
            duration: TimeSpan.FromSeconds(12.345));

        var output = report.ToFormattedString();

        output.ShouldContain("1 pages in 12.3s");
    }

    [Fact]
    public void ToFormattedString_ReturnsSameContentAsWriteTo()
    {
        var diagnostics = ImmutableList.Create(
            new BuildDiagnostic(new DiagnosticWarning(MakeRoute("/warn"), "something")));

        var report = MakeReport(
            diagnostics: diagnostics,
            generatedPages: [MakeRoute("/a")],
            duration: TimeSpan.FromSeconds(2.0));

        var fromToFormattedString = report.ToFormattedString();

        using var writer = new StringWriter();
        report.WriteTo(writer);
        var fromWriteTo = writer.ToString();

        fromToFormattedString.ShouldBe(fromWriteTo);
    }
}
