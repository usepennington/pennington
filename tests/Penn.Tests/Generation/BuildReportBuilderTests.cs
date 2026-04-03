using Penn.Generation;
using Penn.Routing;

namespace Penn.Tests.Generation;

public class BuildReportBuilderTests
{
    private static ContentRoute MakeRoute(string path = "/test") => new()
    {
        CanonicalPath = new UrlPath(path),
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
}
