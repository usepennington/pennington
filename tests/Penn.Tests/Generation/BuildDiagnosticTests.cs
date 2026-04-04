using Penn.Generation;
using Penn.Routing;

namespace Penn.Tests.Generation;

public class BuildDiagnosticTests
{
    private static ContentRoute MakeRoute(string path = "/test") => new()
    {
        CanonicalPath = new UrlPath(path),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
    };

    [Fact]
    public void ExhaustivePatternMatch_AllThreeCases()
    {
        BuildDiagnostic info = new BuildDiagnostic(new DiagnosticInfo(MakeRoute("/a"), "info"));
        BuildDiagnostic warning = new BuildDiagnostic(new DiagnosticWarning(MakeRoute("/b"), "warn"));
        BuildDiagnostic error = new BuildDiagnostic(new DiagnosticError(MakeRoute("/c"), "err", null));

        Describe(info).ShouldBe("Info: /a - info");
        Describe(warning).ShouldBe("Warning: /b - warn");
        Describe(error).ShouldBe("Error: /c - err");
    }

    [Fact]
    public void TypeCheck_IsDiagnosticError()
    {
        var diagnostic = new BuildDiagnostic(new DiagnosticError(MakeRoute(), "fail", null));
        (diagnostic is DiagnosticError).ShouldBeTrue();
        (diagnostic is DiagnosticInfo).ShouldBeFalse();
        (diagnostic is DiagnosticWarning).ShouldBeFalse();
    }

    [Fact]
    public void TypeCheck_IsDiagnosticInfo()
    {
        var diagnostic = new BuildDiagnostic(new DiagnosticInfo(MakeRoute(), "info"));
        (diagnostic is DiagnosticInfo).ShouldBeTrue();
        (diagnostic is DiagnosticError).ShouldBeFalse();
        (diagnostic is DiagnosticWarning).ShouldBeFalse();
    }

    [Fact]
    public void TypeCheck_IsDiagnosticWarning()
    {
        var diagnostic = new BuildDiagnostic(new DiagnosticWarning(MakeRoute(), "warn"));
        (diagnostic is DiagnosticWarning).ShouldBeTrue();
        (diagnostic is DiagnosticInfo).ShouldBeFalse();
        (diagnostic is DiagnosticError).ShouldBeFalse();
    }

    [Fact]
    public void DiagnosticError_ExceptionAccessibleViaPatternMatch()
    {
        var ex = new InvalidOperationException("bad state");
        var diagnostic = new BuildDiagnostic(new DiagnosticError(MakeRoute(), "error", ex));

        var recovered = diagnostic switch
        {
            DiagnosticError e => e.Exception,
            _ => null
        };

        recovered.ShouldNotBeNull();
        recovered.Message.ShouldBe("bad state");
    }

    private static string Describe(BuildDiagnostic d) => d switch
    {
        DiagnosticInfo i    => $"Info: {i.Route.CanonicalPath} - {i.Message}",
        DiagnosticWarning w => $"Warning: {w.Route.CanonicalPath} - {w.Message}",
        DiagnosticError e   => $"Error: {e.Route.CanonicalPath} - {e.Message}",
        _ => throw new InvalidOperationException("Unknown diagnostic case")
    };
}
