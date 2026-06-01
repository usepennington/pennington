namespace Pennington.TreeSitter;

using Resolution;

/// <summary>Configuration for the Pennington tree-sitter integration.</summary>
public sealed class TreeSitterOptions
{
    /// <summary>
    /// Base directory that <c>:symbol</c> file references resolve against. When null or empty,
    /// <see cref="TreeSitterExtensions.AddTreeSitter"/> registers only these options and no services.
    /// </summary>
    public string? ContentRoot { get; set; }

    /// <summary>
    /// Maps a fence base-language id (e.g. <c>python</c>, <c>cs</c>) to the declaration config used to resolve
    /// member name paths. Seeded with built-in defaults; callers may add or replace entries.
    /// </summary>
    public Dictionary<string, LanguageDeclarationConfig> LanguageConfigs { get; } =
        LanguageDeclarationConfigDefaults.CreateDefaults();

    /// <summary>
    /// File globs watched (recursively) under <see cref="ContentRoot"/> for live-reload. Defaults to the
    /// source extensions of the built-in languages plus common markup/data formats (HTML, CSS, JSON, Razor)
    /// that are embedded whole-file — those need no <see cref="LanguageConfigs"/> entry because a bare
    /// <c>:symbol</c> reference returns the file as-is. Watching by extension keeps most build output from
    /// triggering reloads; note <c>*.json</c> and <c>*.cs</c> can still match files under <c>bin/</c>/<c>obj/</c>,
    /// so prefer a focused content root (a dedicated snippets folder) when pointing at a source tree. Add or
    /// remove globs to customize.
    /// </summary>
    public ISet<string> WatchFilePatterns { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "*.cs", "*.py", "*.ts", "*.js", "*.jsx", "*.mjs", "*.cjs",
        "*.java", "*.rs", "*.go", "*.rb", "*.php",
        "*.html", "*.htm", "*.css", "*.json", "*.razor", "*.cshtml",
    };

    /// <summary>Returns the declaration config registered for <paramref name="languageId"/>, or null when none is configured.</summary>
    public LanguageDeclarationConfig? ResolveConfig(string languageId) =>
        LanguageConfigs.GetValueOrDefault(languageId);
}
