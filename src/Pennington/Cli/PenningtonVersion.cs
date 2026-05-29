namespace Pennington.Cli;

using System.Reflection;

/// <summary>Resolves the Pennington package version from assembly metadata for diagnostic output.</summary>
internal static class PenningtonVersion
{
    /// <summary>Informational version with MinVer build metadata trimmed (matches the published NuGet version).</summary>
    public static string Value { get; } = Resolve();

    private static string Resolve()
    {
        var attr = typeof(PenningtonVersion).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        var raw = attr?.InformationalVersion ?? "unknown";
        var plus = raw.IndexOf('+');
        return plus >= 0 ? raw[..plus] : raw;
    }
}
