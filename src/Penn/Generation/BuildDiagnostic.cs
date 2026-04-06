namespace Penn.Generation;

using Penn.Routing;

// Case types
public record DiagnosticInfo(ContentRoute? Route, string Message, string? SourceFile = null);
public record DiagnosticWarning(ContentRoute? Route, string Message, string? SourceFile = null);
public record DiagnosticError(ContentRoute? Route, string Message, Exception? Exception = null, string? SourceFile = null);

// The union
public union BuildDiagnostic(DiagnosticInfo, DiagnosticWarning, DiagnosticError)
{
    public ContentRoute? Route => this switch
    {
        DiagnosticInfo i    => i.Route,
        DiagnosticWarning w => w.Route,
        DiagnosticError e   => e.Route,
        _ => throw new InvalidOperationException()
    };

    public string Message => this switch
    {
        DiagnosticInfo i    => i.Message,
        DiagnosticWarning w => w.Message,
        DiagnosticError e   => e.Message,
        _ => throw new InvalidOperationException()
    };

    public string? SourceFile => this switch
    {
        DiagnosticInfo i    => i.SourceFile,
        DiagnosticWarning w => w.SourceFile,
        DiagnosticError e   => e.SourceFile,
        _ => throw new InvalidOperationException()
    };
}
