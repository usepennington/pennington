namespace Pennington.Markdown.Shortcodes;

using System.Reflection;

/// <summary>
/// Built-in shortcode that emits the running host application's version string.
/// Resolves <see cref="Assembly.GetEntryAssembly"/>, then prefers the assembly's
/// informational version (which captures Git SHA / pre-release suffixes set by
/// MSBuild) and falls back to the file version and finally <c>"unknown"</c> when
/// no entry assembly is available (test hosts, some embedded scenarios).
/// The optional <c>format</c> named argument accepts <c>full</c> (default),
/// <c>major</c>, <c>minor</c>, and <c>informational</c>.
/// </summary>
public sealed class AssemblyVersionShortcode : IShortcode
{
    /// <inheritdoc />
    public string Name => "Version";

    /// <inheritdoc />
    public Task<string> ExecuteAsync(
        ShortcodeInvocation invocation,
        ShortcodeContext context,
        CancellationToken cancellationToken)
    {
        var entry = Assembly.GetEntryAssembly();
        var format = invocation.NamedArgs.TryGetValue("format", out var f) ? f : "full";

        var output = Resolve(entry, format);
        return Task.FromResult(output);
    }

    private static string Resolve(Assembly? entry, string format)
    {
        if (entry is null)
        {
            return "unknown";
        }

        var name = entry.GetName();
        var version = name.Version;

        return format.ToLowerInvariant() switch
        {
            "major" => version is null ? "unknown" : version.Major.ToString(),
            "minor" => version is null ? "unknown" : $"{version.Major}.{version.Minor}",
            "informational" =>
                entry.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? version?.ToString()
                ?? "unknown",
            _ => version?.ToString() ?? "unknown",
        };
    }
}
