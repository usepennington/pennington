namespace Pennington.Book.Composition;

using System.Reflection;

/// <summary>
/// Resolves informational versions for the book's provenance: the host site's version for the cover
/// and the Pennington core version for the colophon credit. Mirrors core's internal
/// <c>PenningtonVersion</c> — MinVer-style <c>+sha</c> build metadata is trimmed so the value matches
/// a published package version.
/// </summary>
internal static class BookVersion
{
    /// <summary>The entry assembly's version — the host site, and by convention the library its docs cover. Null when unresolvable.</summary>
    public static string? EntryAssembly() => Resolve(Assembly.GetEntryAssembly());

    /// <summary>The Pennington core version, for the colophon's "produced with" credit. Null when unresolvable.</summary>
    public static string? Pennington() => Resolve(typeof(Infrastructure.PenningtonOptions).Assembly);

    private static string? Resolve(Assembly? assembly)
    {
        var raw = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var plus = raw.IndexOf('+');
        return plus >= 0 ? raw[..plus] : raw;
    }
}
