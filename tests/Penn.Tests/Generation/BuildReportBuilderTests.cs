using Penn.Generation;
using Penn.Routing;

namespace Penn.Tests.Generation;

public class BuildReportBuilderTests
{
    private static ContentRoute MakeRoute(string path = "/test") => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
    };

    [Fact]
    public void Builder_ProducesValidReport()
    {
        var builder = new BuildReportBuilder();
        var report = builder.Build();

        report.ShouldNotBeNull();
        report.Diagnostics.ShouldBeEmpty();
        report.BrokenLinks.ShouldBeEmpty();
        report.GeneratedPages.ShouldBeEmpty();
        report.SkippedPages.ShouldBeEmpty();
        report.FailedPages.ShouldBeEmpty();
    }

    [Fact]
    public void AddInfo_AddsDiagnosticInfo()
    {
        var builder = new BuildReportBuilder();
        var route = MakeRoute("/info");
        builder.AddInfo(route, "informational");

        var report = builder.Build();
        report.Diagnostics.Count.ShouldBe(1);
        (report.Diagnostics[0] is DiagnosticInfo).ShouldBeTrue();
        report.Diagnostics[0].Message.ShouldBe("informational");
    }

    [Fact]
    public void AddWarning_AddsDiagnosticWarning()
    {
        var builder = new BuildReportBuilder();
        var route = MakeRoute("/warn");
        builder.AddWarning(route, "watch out");

        var report = builder.Build();
        report.Diagnostics.Count.ShouldBe(1);
        (report.Diagnostics[0] is DiagnosticWarning).ShouldBeTrue();
        report.Diagnostics[0].Message.ShouldBe("watch out");
    }

    [Fact]
    public void AddError_AddsDiagnosticError_AndAddsToFailedPages()
    {
        var builder = new BuildReportBuilder();
        var route = MakeRoute("/error");
        builder.AddError(route, "failed", new Exception("boom"));

        var report = builder.Build();
        report.Diagnostics.Count.ShouldBe(1);
        (report.Diagnostics[0] is DiagnosticError).ShouldBeTrue();
        report.Diagnostics[0].Message.ShouldBe("failed");
        report.FailedPages.Count.ShouldBe(1);
        report.FailedPages[0].ShouldBe(route);
    }

    [Fact]
    public void AddGeneratedPage_AddsToGeneratedPages()
    {
        var builder = new BuildReportBuilder();
        var route = MakeRoute("/page");
        builder.AddGeneratedPage(route);

        var report = builder.Build();
        report.GeneratedPages.Count.ShouldBe(1);
        report.GeneratedPages[0].ShouldBe(route);
    }

    [Fact]
    public void AddSkippedPage_AddsToSkippedPages()
    {
        var builder = new BuildReportBuilder();
        var route = MakeRoute("/skipped");
        builder.AddSkippedPage(route);

        var report = builder.Build();
        report.SkippedPages.Count.ShouldBe(1);
        report.SkippedPages[0].ShouldBe(route);
    }

    [Fact]
    public void AddBrokenLink_AddsToBrokenLinks()
    {
        var builder = new BuildReportBuilder();
        var link = new BrokenLink(MakeRoute(), "http://broken.com", LinkType.External, "404");
        builder.AddBrokenLink(link);

        var report = builder.Build();
        report.BrokenLinks.Count.ShouldBe(1);
        report.BrokenLinks[0].ShouldBe(link);
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
        var diagnostic = new BuildDiagnostic(new DiagnosticInfo(MakeRoute(), "direct add"));
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

        // 2 broken links
        builder.AddBrokenLink(new BrokenLink(MakeRoute("/docs/getting-started"), "/docs/install", LinkType.Internal, "Page not found"));
        builder.AddBrokenLink(new BrokenLink(MakeRoute("/blog/hello-world"), "/images/missing.png", LinkType.Image, "Page not found"));

        // 1 warning
        builder.AddWarning(MakeRoute("/docs/old-page"), "redirect target not found");

        // 1 info
        builder.AddInfo(MakeRoute("/docs/api-reference"), "API docs generated from 42 types");

        var report = builder.Build();

        report.GeneratedPages.Count.ShouldBe(5);
        report.SkippedPages.Count.ShouldBe(2);
        report.FailedPages.Count.ShouldBe(1);
        report.BrokenLinks.Count.ShouldBe(2);
        report.TotalPages.ShouldBe(8); // 5 + 2 + 1
        report.HasErrors.ShouldBeTrue(); // errors + broken links

        // Diagnostics breakdown
        report.Diagnostics.Count(d => d is DiagnosticInfo).ShouldBe(1);
        report.Diagnostics.Count(d => d is DiagnosticWarning).ShouldBe(1);
        report.Diagnostics.Count(d => d is DiagnosticError).ShouldBe(1);
    }

    [Fact]
    public void BrokenLinksAlone_MakeHasErrorsTrue()
    {
        var builder = new BuildReportBuilder();
        builder.AddGeneratedPage(MakeRoute("/page"));
        builder.AddBrokenLink(new BrokenLink(MakeRoute("/page"), "/missing", LinkType.Internal, "404"));

        var report = builder.Build();

        report.HasErrors.ShouldBeTrue();
        report.FailedPages.ShouldBeEmpty(); // no failed pages, but broken links → error
    }

    [Fact]
    public void OnlyWarningsAndInfo_HasErrorsFalse()
    {
        var builder = new BuildReportBuilder();
        builder.AddGeneratedPage(MakeRoute("/page"));
        builder.AddWarning(MakeRoute("/old"), "deprecated");
        builder.AddInfo(MakeRoute("/page"), "processed in 50ms");

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

        var error = report.Diagnostics[0] switch { DiagnosticError e => e, _ => null };
        error.ShouldNotBeNull();
        error.Exception.ShouldBe(ex);
        error.Exception!.Message.ShouldBe("NullRef in Render");
    }

    [Fact]
    public void LargeSite_ManyPages_ReportHandlesCorrectly()
    {
        var builder = new BuildReportBuilder();

        for (var i = 0; i < 200; i++)
            builder.AddGeneratedPage(MakeRoute($"/page-{i}"));

        for (var i = 0; i < 10; i++)
            builder.AddSkippedPage(MakeRoute($"/draft-{i}"));

        builder.AddError(MakeRoute("/broken"), "parse error");

        var report = builder.Build();

        report.GeneratedPages.Count.ShouldBe(200);
        report.SkippedPages.Count.ShouldBe(10);
        report.FailedPages.Count.ShouldBe(1);
        report.TotalPages.ShouldBe(211);
        report.HasErrors.ShouldBeTrue();
    }
}
