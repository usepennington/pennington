namespace Pennington.Routing;

/// <summary>Describes the canonical location of a piece of content, its output file, and originating locale.</summary>
public sealed record ContentRoute
{
    /// <summary>Canonical URL path (leading slash, trailing slash for directories).</summary>
    public required UrlPath CanonicalPath { get; init; }

    /// <summary>Relative output file path written during static generation.</summary>
    public required FilePath OutputFile { get; init; }

    /// <summary>Originating source file on disk, if any.</summary>
    public FilePath? SourceFile { get; init; }

    /// <summary>Locale code for this route; empty for the default locale.</summary>
    public string Locale { get; init; } = "";

    /// <summary>True when this route serves default-locale content as a fallback for a missing translation.</summary>
    public bool IsFallback { get; init; }

    /// <summary>Compose the canonical path with the site's canonical base URL; see <see cref="UrlComposer.Combine"/>.</summary>
    public UrlPath AbsoluteUrl(UrlPath canonicalBase) => UrlComposer.Combine(canonicalBase, CanonicalPath);

    /// <summary>True when this route belongs to the default (unprefixed) locale.</summary>
    public bool IsDefaultLocale => string.IsNullOrEmpty(Locale);
}