namespace ExtensibilityLabExample;

using Pennington.LlmsTxt;

/// <summary>
/// Bare-host configuration for <c>penn.AddLlmsTxt(...)</c>. Fenced by the
/// "Bare Pennington" section of how-to/feeds/llms-txt so the example shows
/// the knobs a bare <c>AddPennington</c> consumer must set themselves
/// (DocSite wires these internally).
/// </summary>
public static class LlmsTxtConfiguration
{
    /// <summary>
    /// <c>GenerateFullFile = false</c> for the common per-page-markdown case. Per-page
    /// markdown is co-located as <c>{route}.md</c> beside each page. The
    /// chrome-stripping selector lives on <c>PenningtonOptions.SiteProjection.ContentSelector</c>
    /// (shared with the search index), so it is configured once at the projection layer
    /// rather than per channel.
    /// </summary>
    public static void Configure(LlmsTxtOptions opts)
    {
        opts.GenerateFullFile = false;
    }
}