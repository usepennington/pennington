using System.Collections.Immutable;
using Pennington.Generation;
using Pennington.Routing;

namespace Pennington.Tests.Generation;

public class BuildReportTests
{
    private static ContentRoute MakeRoute(string path = "/test") => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
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
    public void ReportWithGeneratedPagesOnly_HasErrors_IsFalse()
    {
        var report = MakeReport(
            generatedPages: [MakeRoute("/a"), MakeRoute("/b")]);
        report.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public void ReportWithDiagnosticError_HasErrors_IsTrue()
    {
        var diagnostics = ImmutableList.Create(
            new BuildDiagnostic(DiagnosticSeverity.Error, MakeRoute(), "error"));
        var report = MakeReport(diagnostics: diagnostics);
        report.HasErrors.ShouldBeTrue();
    }

    [Fact]
    public void ReportWithBrokenLinks_HasErrors_IsTrue()
    {
        var brokenLinks = ImmutableList.Create(
            new BrokenLink(MakeRoute(), "http://example.com/broken", LinkType.External, "404"));
        var report = MakeReport(brokenLinks: brokenLinks);
        report.HasErrors.ShouldBeTrue();
    }

    [Fact]
    public void ReportWithFailedPages_HasErrors_IsTrue()
    {
        var report = MakeReport(failedPages: [MakeRoute("/failed")]);
        report.HasErrors.ShouldBeTrue();
    }

    [Fact]
    public void ReportWithOnlyInfoAndWarning_HasErrors_IsFalse()
    {
        var diagnostics = ImmutableList.Create(
            new BuildDiagnostic(DiagnosticSeverity.Info, MakeRoute("/a"), "info"),
            new BuildDiagnostic(DiagnosticSeverity.Warning, MakeRoute("/b"), "warn"));
        var report = MakeReport(diagnostics: diagnostics);
        report.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public void TotalPages_SumsGeneratedSkippedFailed()
    {
        var report = MakeReport(
            generatedPages: [MakeRoute("/a"), MakeRoute("/b")],
            skippedPages: [MakeRoute("/c")],
            failedPages: [MakeRoute("/d"), MakeRoute("/e"), MakeRoute("/f")]);
        report.TotalPages.ShouldBe(6);
    }

}
