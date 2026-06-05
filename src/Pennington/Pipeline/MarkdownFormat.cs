namespace Pennington.Pipeline;

/// <summary>
/// Dispatch-key conventions for the built-in markdown sources. Each source registered via
/// <c>AddMarkdownContent</c> gets a distinct key (<see cref="SourceKey"/>) so the pipeline routes
/// it to that source's front-matter parser instead of a single shared one — they all still render
/// through the one markdown renderer. <see cref="Matches"/> recognises any markdown key, for
/// consumers that ask "is this a markdown file?" rather than which source produced it.
/// </summary>
public static class MarkdownFormat
{
    /// <summary>Key for the first markdown source, and the standalone key for single-source hosts.</summary>
    public const string Key = "markdown";

    /// <summary>Distinct dispatch key for the markdown source at <paramref name="index"/> in registration order (index 0 keeps the bare <see cref="Key"/>).</summary>
    public static string SourceKey(int index) => index == 0 ? Key : $"{Key}#{index}";

    /// <summary>True when <paramref name="format"/> is one of the markdown dispatch keys produced by <see cref="SourceKey"/>.</summary>
    public static bool Matches(string format) =>
        string.Equals(format, Key, StringComparison.Ordinal)
        || format.StartsWith(Key + "#", StringComparison.Ordinal);
}
