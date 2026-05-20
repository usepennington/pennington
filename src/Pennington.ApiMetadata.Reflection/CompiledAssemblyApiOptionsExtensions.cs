namespace Pennington.ApiMetadata.Reflection;

using System.IO;
using System.Reflection;

/// <summary>Sugar for <see cref="CompiledAssemblyApiOptions"/> that resolves documented assemblies via <see cref="Assembly"/> instead of filesystem paths.</summary>
public static class CompiledAssemblyApiOptionsExtensions
{
    /// <summary>
    /// Resolves <paramref name="assemblySimpleName"/> in the host application's default
    /// load context (populated from the project's <c>.deps.json</c> and the NuGet cache),
    /// then adds the resolved <c>.dll</c> path to
    /// <see cref="CompiledAssemblyApiOptions.AssemblyFiles"/>. Requires a matching
    /// <c>&lt;PackageReference&gt;</c> in the docsite project; no source reference to any
    /// type in the package is needed.
    /// </summary>
    /// <remarks>
    /// The companion <c>.xml</c> file ships alongside the <c>.dll</c> in NuGet packages
    /// built with <c>&lt;GenerateDocumentationFile&gt;true&lt;/GenerateDocumentationFile&gt;</c>.
    /// MSBuild does not copy xmldoc files into <c>bin/</c> by default, so when the xml is
    /// missing next to the resolved bin-path DLL we fall back to the NuGet cache copy
    /// (where the package originally ships both files together).
    /// </remarks>
    /// <exception cref="FileNotFoundException">No package-referenced assembly matches <paramref name="assemblySimpleName"/>.</exception>
    /// <exception cref="InvalidOperationException">The assembly loaded but exposes no file location (e.g. single-file bundle). Use <see cref="CompiledAssemblyApiOptions.AssemblyFiles"/> with an explicit path instead.</exception>
    public static CompiledAssemblyApiOptions FromPackageReference(
        this CompiledAssemblyApiOptions options, string assemblySimpleName)
    {
        var asm = Assembly.Load(new AssemblyName(assemblySimpleName));
        var location = asm.Location;
        if (string.IsNullOrEmpty(location))
        {
            throw new InvalidOperationException(
                $"Assembly '{assemblySimpleName}' resolved but has no file location — " +
                "likely a single-file bundle. Stage the .dll and .xml manually and use AssemblyFiles instead.");
        }

        var xmlAtLocation = Path.ChangeExtension(location, ".xml");
        if (!File.Exists(xmlAtLocation) && TryResolveFromNuGetCache(asm, assemblySimpleName) is { } cached)
        {
            location = cached;
        }

        options.AssemblyFiles.Add(location);
        return options;
    }

    private static string? TryResolveFromNuGetCache(Assembly asm, string assemblySimpleName)
    {
        var packagesRoot = Environment.GetEnvironmentVariable("NUGET_PACKAGES")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
        if (!Directory.Exists(packagesRoot))
        {
            return null;
        }

        var packageDir = Path.Combine(packagesRoot, assemblySimpleName.ToLowerInvariant());
        if (!Directory.Exists(packageDir))
        {
            return null;
        }

        var asmVersion = asm.GetName().Version;
        var versionDirs = Directory.EnumerateDirectories(packageDir)
            .OrderByDescending(d => asmVersion is not null
                && Path.GetFileName(d).StartsWith(asmVersion.ToString(3), StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(d => d, StringComparer.OrdinalIgnoreCase);

        foreach (var versionDir in versionDirs)
        {
            var libRoot = Path.Combine(versionDir, "lib");
            if (!Directory.Exists(libRoot))
            {
                continue;
            }

            foreach (var tfmDir in Directory.EnumerateDirectories(libRoot).OrderByDescending(d => d, StringComparer.OrdinalIgnoreCase))
            {
                var dll = Path.Combine(tfmDir, assemblySimpleName + ".dll");
                var xml = Path.Combine(tfmDir, assemblySimpleName + ".xml");
                if (File.Exists(dll) && File.Exists(xml))
                {
                    return dll;
                }
            }
        }
        return null;
    }
}