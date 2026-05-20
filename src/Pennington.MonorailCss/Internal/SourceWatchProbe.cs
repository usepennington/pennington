using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Pennington.MonorailCss.Internal;

/// <summary>
/// Resolves the on-disk source-project directories that produced the loaded non-system
/// assemblies, by reading portable PDB document paths and walking each path up to its owning
/// <c>.csproj</c>. Used to populate <c>MonorailDiscoveryOptions.WatchSourceDirectories</c> so
/// <c>dotnet watch</c> hot-reload picks up <c>.razor</c>/<c>.cs</c> edits in referenced
/// projects whose source lives outside <c>IHostEnvironment.ContentRootPath</c>.
/// </summary>
internal static class SourceWatchProbe
{
    /// <summary>
    /// Force-loads the entry assembly's transitive references and returns the deduped set of
    /// source-project directories derived from PDB documents. Best-effort: assemblies with no
    /// PDB, missing source paths, or unreachable directories are silently skipped.
    /// </summary>
    public static ImmutableArray<string> GetProjectDirectories()
    {
        ForceLoadTransitiveReferences();

        var dirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (IsSystemAssembly(asm))
            {
                continue;
            }

            var docs = TryReadDocumentPaths(asm);
            if (docs is null)
            {
                continue;
            }

            foreach (var path in docs)
            {
                var projectDir = FindProjectRoot(path);
                if (projectDir is not null)
                {
                    dirs.Add(projectDir);
                }
            }
        }

        return [.. dirs];
    }

    private static void ForceLoadTransitiveReferences()
    {
        var entry = Assembly.GetEntryAssembly();
        if (entry is null)
        {
            return;
        }

        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var stack = new Stack<Assembly>();
        stack.Push(entry);
        visited.Add(entry.GetName().Name ?? string.Empty);

        while (stack.Count > 0)
        {
            var asm = stack.Pop();
            if (IsSystemAssembly(asm))
            {
                continue;
            }

            AssemblyName[] refs;
            try { refs = asm.GetReferencedAssemblies(); }
            catch { continue; }

            foreach (var refName in refs)
            {
                var name = refName.Name;
                if (string.IsNullOrEmpty(name) || !visited.Add(name))
                {
                    continue;
                }

                if (IsSystemName(name))
                {
                    continue;
                }

                try
                {
                    var loaded = Assembly.Load(refName);
                    stack.Push(loaded);
                }
                catch
                {
                    // Resource assemblies, NIs, missing optional refs — skip.
                }
            }
        }
    }

    private static IReadOnlyList<string>? TryReadDocumentPaths(Assembly assembly)
    {
        string location;
        try { location = assembly.Location; }
        catch { return null; }

        if (string.IsNullOrEmpty(location) || !File.Exists(location))
        {
            return null;
        }

        try
        {
            using var peStream = File.OpenRead(location);
            using var peReader = new PEReader(peStream);

            MetadataReaderProvider? provider = null;
            FileStream? pdbStream = null;
            try
            {
                foreach (var entry in peReader.ReadDebugDirectory())
                {
                    if (entry.Type != DebugDirectoryEntryType.EmbeddedPortablePdb)
                    {
                        continue;
                    }

                    provider = peReader.ReadEmbeddedPortablePdbDebugDirectoryData(entry);
                    break;
                }

                if (provider is null)
                {
                    var pdbPath = Path.ChangeExtension(location, ".pdb");
                    if (File.Exists(pdbPath))
                    {
                        pdbStream = File.OpenRead(pdbPath);
                        provider = MetadataReaderProvider.FromPortablePdbStream(
                            pdbStream,
                            MetadataStreamOptions.PrefetchMetadata);
                    }
                }

                if (provider is null)
                {
                    return null;
                }

                var reader = provider.GetMetadataReader();
                var paths = new List<string>(reader.Documents.Count);
                foreach (var docHandle in reader.Documents)
                {
                    if (docHandle.IsNil)
                    {
                        continue;
                    }

                    var doc = reader.GetDocument(docHandle);
                    paths.Add(reader.GetString(doc.Name));
                }
                return paths;
            }
            finally
            {
                provider?.Dispose();
                pdbStream?.Dispose();
            }
        }
        catch
        {
            return null;
        }
    }

    private static string? FindProjectRoot(string sourceFilePath)
    {
        try
        {
            var dir = Path.GetDirectoryName(sourceFilePath);
            while (!string.IsNullOrEmpty(dir))
            {
                if (Directory.Exists(dir) &&
                    Directory.EnumerateFiles(dir, "*.csproj").Any())
                {
                    return dir;
                }
                dir = Path.GetDirectoryName(dir);
            }
        }
        catch
        {
            // Mangled or build-machine paths from third-party PDBs — skip.
        }
        return null;
    }

    private static bool IsSystemAssembly(Assembly assembly)
    {
        if (assembly.IsDynamic)
        {
            return true;
        }

        return IsSystemName(assembly.GetName().Name);
    }

    private static bool IsSystemName(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return true;
        }

        return name.StartsWith("System.", StringComparison.Ordinal)
            || name.StartsWith("Microsoft.", StringComparison.Ordinal)
            || name == "mscorlib"
            || name == "netstandard"
            || name == "WindowsBase"
            || name == "MonorailCss"
            || name == "MonorailCss.Discovery";
    }
}