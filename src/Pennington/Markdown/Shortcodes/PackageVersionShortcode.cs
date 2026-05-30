namespace Pennington.Markdown.Shortcodes;

/// <summary>
/// Built-in shortcode that emits Pennington's published package version, dispatched by
/// the name <c>PackageVersion</c>. Resolves the Pennington core assembly's
/// <see cref="System.Reflection.AssemblyInformationalVersionAttribute"/> and trims MinVer's
/// <c>+&lt;sha&gt;</c> build metadata, so it matches the NuGet version a reader installs.
/// Unlike the <c>Version</c> shortcode (which reads the host's entry assembly), this always
/// reports Pennington itself — the value to stamp into install snippets. Takes no arguments.
/// </summary>
public sealed class PackageVersionShortcode : IShortcode
{
    /// <inheritdoc />
    public string Name => "PackageVersion";

    /// <inheritdoc />
    public Task<string> ExecuteAsync(
        ShortcodeInvocation invocation,
        ShortcodeContext context,
        CancellationToken cancellationToken) =>
        Task.FromResult(PenningtonVersion.Value);
}
