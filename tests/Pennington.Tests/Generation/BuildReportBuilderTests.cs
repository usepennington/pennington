using Pennington.Generation;
using Pennington.Routing;

namespace Pennington.Tests.Generation;

public class BuildReportBuilderTests
{
    private static ContentRoute MakeRoute(string path = "/test") => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
    };

    [Fact]
    public void AddError_AddsDiagnosticError_AndAddsToFailedPages()
    {
        var builder = new BuildReportBuilder();
        var route = MakeRoute("/error");
        builder.AddError(route, "failed", new Exception("boom"));

        var report = builder.Build();
        report.Diagnostics.Count.ShouldBe(1);
        report.Diagnostics[0].Severity.ShouldBe(DiagnosticSeverity.Error);
        report.Diagnostics[0].Message.ShouldBe("failed");
        report.FailedPages.Count.ShouldBe(1);
        report.FailedPages[0].ShouldBe(route);
    }

    [Fact]
    public void Duration_IsPositive()
    {
        var builder = new BuildReportBuilder();
        // Do a small amount of work to ensure some time elapses
        builder.AddGeneratedPage(MakeRoute("/a"));
        var report = builder.Build();

        report.Duration.ShouldBeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public void AddDiagnostic_AddsDirectly()
    {
        var builder = new BuildReportBuilder();
        var diagnostic = new BuildDiagnostic(DiagnosticSeverity.Warning, MakeRoute(), "direct add");
        builder.AddDiagnostic(diagnostic);

        var report = builder.Build();
        report.Diagnostics.Count.ShouldBe(1);
        report.Diagnostics[0].Message.ShouldBe("direct add");
    }

    // --- Realistic scenario tests ---

    [Fact]
    public void RealisticBuild_MixedOutcomes_ReportReflectsAll()
    {
        var builder = new BuildReportBuilder();

        // 5 successful pages
        builder.AddGeneratedPage(MakeRoute("/docs/getting-started"));
        builder.AddGeneratedPage(MakeRoute("/docs/configuration"));
        builder.AddGeneratedPage(MakeRoute("/docs/api-reference"));
        builder.AddGeneratedPage(MakeRoute("/blog/hello-world"));
        builder.AddGeneratedPage(MakeRoute("/about"));

        // 2 drafts skipped
        builder.AddSkippedPage(MakeRoute("/blog/wip-post"));
        builder.AddSkippedPage(MakeRoute("/docs/unreleased-feature"));

        // 1 failed page
        builder.AddError(MakeRoute("/docs/broken"), "YAML parse error at line 4", new Exception("bad yaml"));

        // 1 warning
        builder.AddWarning(MakeRoute("/docs/old-page"), "redirect target not found");

        var report = builder.Build();

        report.GeneratedPages.Count.ShouldBe(5);
        report.SkippedPages.Count.ShouldBe(2);
        report.FailedPages.Count.ShouldBe(1);
        report.TotalPages.ShouldBe(8); // 5 + 2 + 1
        report.HasErrors.ShouldBeTrue();

        // Diagnostics breakdown
        report.Diagnostics.Count(d => d.Severity is DiagnosticSeverity.Warning).ShouldBe(1);
        report.Diagnostics.Count(d => d.Severity is DiagnosticSeverity.Error).ShouldBe(1);
    }

    [Fact]
    public void OnlyWarnings_HasErrorsFalse()
    {
        var builder = new BuildReportBuilder();
        builder.AddGeneratedPage(MakeRoute("/page"));
        builder.AddWarning(MakeRoute("/old"), "deprecated");

        var report = builder.Build();

        report.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public void MultipleErrors_AllPreservedInOrder()
    {
        var builder = new BuildReportBuilder();
        builder.AddError(MakeRoute("/fail-1"), "Error 1");
        builder.AddError(MakeRoute("/fail-2"), "Error 2");
        builder.AddError(MakeRoute("/fail-3"), "Error 3");

        var report = builder.Build();

        report.FailedPages.Count.ShouldBe(3);
        report.Diagnostics.Count.ShouldBe(3);
        report.Diagnostics[0].Message.ShouldBe("Error 1");
        report.Diagnostics[1].Message.ShouldBe("Error 2");
        report.Diagnostics[2].Message.ShouldBe("Error 3");
    }

    [Fact]
    public void ErrorWithException_ExceptionPreserved()
    {
        var builder = new BuildReportBuilder();
        var ex = new InvalidOperationException("NullRef in Render");
        builder.AddError(MakeRoute("/crash"), "Render failed", ex);

        var report = builder.Build();

        report.Diagnostics[0].Severity.ShouldBe(DiagnosticSeverity.Error);
        report.Diagnostics[0].Exception.ShouldBe(ex);
        report.Diagnostics[0].Exception!.Message.ShouldBe("NullRef in Render");
    }

    [Fact]
    public void LargeSite_ManyPages_ReportHandlesCorrectly()
    {
        var builder = new BuildReportBuilder();

        for (var i = 0; i < 200; i++)
        {
            builder.AddGeneratedPage(MakeRoute($"/page-{i}"));
        }

        for (var i = 0; i < 10; i++)
        {
            builder.AddSkippedPage(MakeRoute($"/draft-{i}"));
        }

        builder.AddError(MakeRoute("/broken"), "parse error");

        var report = builder.Build();

        report.GeneratedPages.Count.ShouldBe(200);
        report.SkippedPages.Count.ShouldBe(10);
        report.FailedPages.Count.ShouldBe(1);
        report.TotalPages.ShouldBe(211);
        report.HasErrors.ShouldBeTrue();
    }
}