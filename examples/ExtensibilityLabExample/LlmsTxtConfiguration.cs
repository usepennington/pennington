namespace ExtensibilityLabExample;

using Pennington.LlmsTxt;

/// <summary>
/// Bare-host configuration for <c>penn.AddLlmsTxt(...)</c>. Fenced by step 3
/// of how-to/configuration/llms-txt so the example shows the three knobs
/// a bare <c>AddPennington</c> consumer must set themselves (DocSite wires
/// these internally).
/// </summary>
public static class LlmsTxtConfiguration
{
    /// <summary>
    /// Default output directory, an <c>article</c> scoping selector matching
    /// the Lab's minimal HTML template, and <c>GenerateFullFile = false</c>
    /// for the common per-page-sidecar case.
    /// </summary>
    public static void Configure(LlmsTxtOptions opts)
    {
        opts.OutputDirectory = "_llms";
        opts.ContentSelector = "article";
        opts.GenerateFullFile = false;
    }
}