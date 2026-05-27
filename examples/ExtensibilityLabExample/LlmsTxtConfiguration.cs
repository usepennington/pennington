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
    /// Default output directory and <c>GenerateFullFile = false</c> for the
    /// common per-page-sidecar case. The chrome-stripping selector lives on
    /// <c>PenningtonOptions.SiteProjection.ContentSelector</c> (shared with
    /// the search index), so it is configured once at the projection layer
    /// rather than per channel.
    /// </summary>
    public static void Configure(LlmsTxtOptions opts)
    {
        opts.OutputDirectory = "_llms";
        opts.GenerateFullFile = false;
    }
}