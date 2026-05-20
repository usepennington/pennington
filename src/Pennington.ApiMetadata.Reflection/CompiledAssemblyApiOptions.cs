namespace Pennington.ApiMetadata.Reflection;

using System.Collections.Generic;

/// <summary>Options for <see cref="CompiledAssemblyApiMetadataProvider"/>.</summary>
public sealed class CompiledAssemblyApiOptions
{
    /// <summary>
    /// Directories to scan for <c>*.dll</c> files with matching <c>*.xml</c> xmldoc files.
    /// Every matched pair in each directory contributes types to the provider. Use this
    /// when every <c>.dll</c> in the folder is intended for documentation — the typical
    /// NuGet <c>lib/&lt;tfm&gt;/</c> layout.
    /// </summary>
    public IList<string> AssemblyDirectories { get; } = [];

    /// <summary>
    /// Explicit <c>.dll</c> paths to document. Use this when a folder contains more
    /// assemblies than you want documented (e.g. dependencies copied alongside the
    /// target for <see cref="System.Reflection.MetadataLoadContext"/> resolution).
    /// The companion <c>.xml</c> file is loaded from the same directory. When both
    /// <see cref="AssemblyDirectories"/> and <see cref="AssemblyFiles"/> are set, both
    /// contribute.
    /// </summary>
    public IList<string> AssemblyFiles { get; } = [];
}