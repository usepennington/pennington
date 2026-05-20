namespace FocusedCodeSamplesExample;

using System.Text;

/// <summary>
/// Minimal single-slot <see cref="StringBuilder"/> pool used by
/// <see cref="ModularWordCounter.FormatV2"/> to demonstrate a before/after
/// xmldocid-diff fence. Not thread-safe; exists as a compilable fixture,
/// not a pattern to copy into production code.
/// </summary>
public static class StringBuilderPool
{
    private static StringBuilder? _cached;

    /// <summary>Rents a cleared <see cref="StringBuilder"/>, creating a new one if none is cached.</summary>
    public static StringBuilder Get()
    {
        var sb = _cached ?? new StringBuilder();
        _cached = null;
        sb.Clear();
        return sb;
    }

    /// <summary>Returns <paramref name="sb"/> to the pool for the next caller to reuse.</summary>
    public static void Return(StringBuilder sb) => _cached = sb;
}