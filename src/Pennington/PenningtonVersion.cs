namespace Pennington;

using System.Reflection;

/// <summary>Resolves the Pennington package version from assembly metadata — the single source shared by llms.txt generation, the <c>diag info</c> command, and the <c>PackageVersion</c> shortcode.</summary>
internal static class PenningtonVersion
{
    /// <summary>Informational version with MinVer build metadata trimmed (matches the published NuGet version).</summary>
    public static string Value { get; } = Resolve();

    private static string Resolve()
    {
        var attr = typeof(PenningtonVersion).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        var raw = attr?.InformationalVersion ?? "unknown";
        // MinVer appends "+<sha>" build metadata; trim it so the value matches the published NuGet PackageVersion.
        var plus = raw.IndexOf('+');
        return plus >= 0 ? raw[..plus] : raw;
    }
}
