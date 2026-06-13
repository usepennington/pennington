namespace Pennington.Favicon;

using System.Collections.Immutable;

/// <summary>
/// Host configuration for favicon / icon <c>&lt;link&gt;</c> tags emitted into the document head. The
/// icon files themselves are user-provided static assets served and copied by the content-root
/// static-files mechanism; this only emits the discovery markup. Set this on
/// <see cref="Infrastructure.PenningtonOptions.Favicons"/> (templates forward it from their own
/// options) to enable the feature; leaving it null disables it.
/// </summary>
public sealed record FaviconOptions
{
    /// <summary>The icon links to emit, in document order. Empty means nothing is emitted.</summary>
    public ImmutableArray<FaviconLink> Icons { get; init; } = [];
}

/// <summary>
/// One icon <c>&lt;link&gt;</c>. The generic <see cref="Rel"/>/<see cref="Href"/>/<see cref="Type"/>/
/// <see cref="Sizes"/>/<see cref="Color"/> model covers <c>rel="icon"</c> (multiple sizes/types),
/// <c>apple-touch-icon</c>, <c>mask-icon</c>, and <c>manifest</c> without special-casing.
/// </summary>
/// <param name="Href">The icon URL. A root-relative href is sub-path prefixed at serve/build time; an absolute href is left as-is.</param>
public sealed record FaviconLink(string Href)
{
    /// <summary>The <c>rel</c> attribute; defaults to <c>icon</c>.</summary>
    public string Rel { get; init; } = "icon";

    /// <summary>The MIME <c>type</c>; inferred from the <see cref="Href"/> extension when null.</summary>
    public string? Type { get; init; }

    /// <summary>The <c>sizes</c> attribute (e.g. <c>32x32</c>, <c>any</c>), or null to omit.</summary>
    public string? Sizes { get; init; }

    /// <summary>The <c>color</c> attribute used by <c>rel="mask-icon"</c>, or null to omit.</summary>
    public string? Color { get; init; }
}
