namespace Pennington.FrontMatter;

/// <summary>Options that control front-matter parsing behavior.</summary>
public sealed class FrontMatterParserOptions
{
    /// <summary>
    /// When true, unknown YAML keys cause the deserializer to throw a
    /// <c>YamlException</c> instead of silently dropping the value. Independently of
    /// this flag, every unknown key is reported as a <c>Warning</c>-severity diagnostic
    /// via <see cref="Diagnostics.DiagnosticContext"/> so dev overlays and build
    /// reports surface typos. Defaults to <c>false</c> (lenient); the engine flips
    /// the default to <c>true</c> in build mode.
    /// </summary>
    public bool StrictUnknownKeys { get; set; } = false;
}
