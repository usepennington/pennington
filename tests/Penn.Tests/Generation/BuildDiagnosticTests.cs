using Penn.Generation;
using Penn.Routing;

namespace Penn.Tests.Generation;

public class BuildDiagnosticTests
{
    private static ContentRoute MakeRoute(string path = "/test") => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
    };

    [Fact]
    public void SeverityCheck_Info()
    {
        var diagnostic = new BuildDiagnostic(DiagnosticSeverity.Info, MakeRoute(), "info");
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Info);
    }

    [Fact]
    public void SeverityCheck_Warning()
    {
        var diagnostic = new BuildDiagnostic(DiagnosticSeverity.Warning, MakeRoute(), "warn");
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Warning);
    }

    [Fact]
    public void SeverityCheck_Error()
    {
        var diagnostic = new BuildDiagnostic(DiagnosticSeverity.Error, MakeRoute(), "err");
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
    }

    [Fact]
    public void Describe_AllThreeSeverities()
    {
        var info = new BuildDiagnostic(DiagnosticSeverity.Info, MakeRoute("/a"), "info");
        var warning = new BuildDiagnostic(DiagnosticSeverity.Warning, MakeRoute("/b"), "warn");
        var error = new BuildDiagnostic(DiagnosticSeverity.Error, MakeRoute("/c"), "err");

        Describe(info).ShouldBe("Info: /a/ - info");
        Describe(warning).ShouldBe("Warning: /b/ - warn");
        Describe(error).ShouldBe("Error: /c/ - err");
    }

    [Fact]
    public void DiagnosticError_ExceptionAccessible()
    {
        var ex = new InvalidOperationException("bad state");
        var diagnostic = new BuildDiagnostic(DiagnosticSeverity.Error, MakeRoute(), "error", ex);

        diagnostic.Exception.ShouldNotBeNull();
        diagnostic.Exception.Message.ShouldBe("bad state");
    }

    private static string Describe(BuildDiagnostic d) => d.Severity switch
    {
        DiagnosticSeverity.Info    => $"Info: {d.Route!.CanonicalPath} - {d.Message}",
        DiagnosticSeverity.Warning => $"Warning: {d.Route!.CanonicalPath} - {d.Message}",
        DiagnosticSeverity.Error   => $"Error: {d.Route!.CanonicalPath} - {d.Message}",
        _ => throw new InvalidOperationException("Unknown severity")
    };
}
