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
    public void ConstructFromDiagnosticInfo_VerifyRouteAndMessage()
    {
        var route = MakeRoute("/info-page");
        var info = new DiagnosticInfo(route, "Info message");
        var diagnostic = new BuildDiagnostic(info);

        diagnostic.Route.ShouldBe(route);
        diagnostic.Message.ShouldBe("Info message");
    }

    [Fact]
    public void ConstructFromDiagnosticWarning_VerifyRouteAndMessage()
    {
        var route = MakeRoute("/warn-page");
        var warning = new DiagnosticWarning(route, "Warning message");
        var diagnostic = new BuildDiagnostic(warning);

        diagnostic.Route.ShouldBe(route);
        diagnostic.Message.ShouldBe("Warning message");
    }

    [Fact]
    public void ConstructFromDiagnosticError_VerifyRouteMessageAndException()
    {
        var route = MakeRoute("/error-page");
        var ex = new InvalidOperationException("something broke");
        var error = new DiagnosticError(route, "Error message", ex);
        var diagnostic = new BuildDiagnostic(error);

        diagnostic.Route.ShouldBe(route);
        diagnostic.Message.ShouldBe("Error message");
    }

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
